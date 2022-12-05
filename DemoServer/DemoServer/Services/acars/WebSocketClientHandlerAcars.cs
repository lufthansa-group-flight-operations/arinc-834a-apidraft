using DemoServer.DataAccess;
using DemoServer.Models;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DemoServer.Services.acars
{
    public class WebSocketClientHandlerAcars : IWebSocketClientHandlerAcars
    {
        private readonly ILogger<WebSocketClientHandlerAcars> _logger;
        private readonly IAcarsMessageService _acars;
        private readonly JsonSerializerOptions _serializerOption;
        private WebSocket _socket;
        private HttpContext _context;
        private CancellationTokenSource _tokenSource;

        public WebSocketClientHandlerAcars(ILogger<WebSocketClientHandlerAcars> logger, IAcarsMessageService acars)
        {

            _logger = logger;
            _acars = acars;
            _serializerOption = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                Converters =
                { new JsonStringEnumConverter()}
            };
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Receive here the message from the ACARS service. This Method is called due to the subscription.
        /// </summary>
        /// <param name="msg"></param>
        public void ReceiveDownlinkUpdate(AcarsDownlink msg, bool includePayload = false)
        {
            // If this is an update of an created message, inform all clients
            if (msg.State == AcarsDownlinkState.queued)
            {
                includePayload = true;
            }
            
            if (includePayload)
            {
                SendData(new AcarsEnvelope("acars_downlink_update", msg));
                return;
            }

            // Remove the payload and send data.
            msg.Payload = null;
            SendData(new AcarsEnvelope("acars_downlink_update", msg));

        }

        public void ReceiveUplinkUpdate(AcarsUplink msg, bool includePayload = false)
        {
            // Filter depricated since one subscription url for all
            //if (_context.Request.Path.ToString().Contains("uplink"))
            {
                SendData(new AcarsEnvelope("acars_uplink_update", msg));
            }
        }

        public void ReceiveStatusUpdate(object msg)
        {
            //SendData(new AcarsEnvelope("acars_status_update", msg));
        }

        public async Task StartListen(WebSocket webSocket, HttpContext httpContext)
        {
            _socket = webSocket;
            _context = httpContext;
            _tokenSource = new CancellationTokenSource();

            // Subscribe to the ACARS messaging service
            _acars.Subscribe(this);

            // TODO: actually handle subscription correctly
            await Subscribe();

            await StartEventLoop();
        }

        private async Task StartEventLoop()
        {
            // 64k buffer for incoming messages
            var buffer = new byte[1 << 16];


            while (_socket.State == WebSocketState.Open)
            {
                var rc = await _socket.ReceiveAsync(buffer, _tokenSource?.Token ?? CancellationToken.None);
                if (rc.EndOfMessage)
                {
                    var msg = string.Empty;
                    switch (rc.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            {
                                try
                                {
                                    msg = Encoding.UTF8.GetString(buffer, 0, rc.Count);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError("Received Non-UTF8 data, closing websocket");
                                    await CloseWebSocketAsync(WebSocketSubscriptionErrorCode.BadFormat);
                                    return;
                                }

                                await HandleMessage(msg);
                            }
                            break;
                        case WebSocketMessageType.Binary:

                            break;
                        case WebSocketMessageType.Close:
                            _logger.LogDebug("Websocket close received");
                            break;
                        default:
                            break;
                    }

                    var ip = _context.Connection.RemoteIpAddress;
                    _logger.LogInformation($"Message [{msg}] received from [{ip}]");
                }
            }
            _logger.LogInformation("Websocket is closed");
        }

        private async Task HandleMessage(string msg)
        {
            // TODO
            // Imagine the subscription would have beeen handled here.
            // Send all up- and downlinks to the client
            
        }

        private async Task Subscribe()
        {
            foreach (var uplink in _acars.Uplinks)
            {
                ReceiveUplinkUpdate(uplink, true);
            }

            foreach (var downlink in _acars.Downlinks)
            {
                ReceiveDownlinkUpdate(downlink, true);
            }
            return;
        }

        private async Task CloseWebSocketAsync(WebSocketSubscriptionErrorCode reason)
        {
            try
            {
                _logger.LogWarning($"Closing connection due to: {reason}");
                await _socket.CloseAsync((WebSocketCloseStatus)reason, reason.ToString(), _tokenSource.Token);
            }
            catch (Exception)
            {
                //Intionally left blank
                _logger.LogError("Could not close WebSocket, Client died earlier");
            }
        }

        private void SendData(object jsonObject)
        {
            try
            {
                var message = JsonSerializer.Serialize(jsonObject, _serializerOption);
                SendData(message);
            }
            catch (Exception)
            {

                //typical 'should never happen' error
                _logger.LogError("Could not Serialize Object");
                throw;
            }
        }

        private void SendData(string data)
        {
            var buffer = Encoding.UTF8.GetBytes(data);

            Task.Run(
                async () =>
                {
                    await _socket.SendAsync(
                        buffer,
                        WebSocketMessageType.Text,
                        true,
                        _tokenSource?.Token ?? CancellationToken.None);
                }
                ).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        _logger.LogError("Send failed", t.Exception);
                        _tokenSource?.Cancel();
                    }
                }).ConfigureAwait(false);
        }
    }
}
