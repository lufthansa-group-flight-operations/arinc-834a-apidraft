//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;

namespace DemoServer.Controllers
{
    [Route("api/v1")]
    [ApiController]
    [Produces("text/plain")]
    public class StapController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly string _stapServerIpAddress;
        private readonly int _stapServerIpPort;

        public StapController(ILogger<StapController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
            _stapServerIpAddress = _config["StapServerIpAddress"];
            _stapServerIpPort = int.Parse(_config["StapServerIpPort"]);
        }

        /// <summary>
        /// WebSocket connection to access a STAP Server.
        /// </summary>
        /// <response code="101">Switching Protocols to Websocket.</response>
        /// <response code="400">Resource can only be accessed with Websockets.</response>
        /// <response code="500">STAP Server is not reachable</response>.
        /// <returns>500 if STAP Server is not reachable.</returns>
        [HttpGet]
        [Route("stap")]
        [ProducesResponseType((int)StatusCodes.Status101SwitchingProtocols)]
        [ProducesResponseType((int)StatusCodes.Status400BadRequest)]
        [ProducesResponseType((int)StatusCodes.Status500InternalServerError)]
        [OpenApiOperation("GetStapConnection")]
        public virtual async Task<IActionResult> GetStapConnection()
        {
            if (this.HttpContext.WebSockets.IsWebSocketRequest)
            {
                _logger.LogDebug("WebSocket Request");
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Task.Run(async () => await HandleWebSocket(HttpContext, webSocket)).ConfigureAwait(false);
                _logger.LogDebug("End of WebSocket handling");
                return new EmptyResult();
            }
            else
            {
                return BadRequest("This Resource can only be accessed with WebSockets");
            }
        }

        private async Task HandleWebSocket(HttpContext context, WebSocket webSocket)
        {
            var tcpClient = new TcpClient();
            // Connect to the "local" stap server.
            try
            {
                tcpClient.Connect(_stapServerIpAddress, _stapServerIpPort);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not connect to STAP server: [0]", ex.Message);
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Stap Server not reachable", CancellationToken.None);
                return;
            }

            var stream = tcpClient.GetStream();

            // Handle incoming data from websocket client and forward it to STAP server
            var webSocketHandleTask = HandleWebSocketData(stream, webSocket);

            // Handle incoming data form STAP server and forward it to websocket client.
            var handleStapTaks = HandleStapServerData(tcpClient, stream, webSocket);

            // Wait until all Tasks for Websockets are ended.
            await Task.WhenAll(webSocketHandleTask, handleStapTaks);
        }

        /// <summary>
        /// Handles Incoming Data From WebSocket Client
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        private async Task HandleWebSocketData(NetworkStream stream, WebSocket webSocket)
        {
            _logger.LogDebug("Start HandleWebSocketData");
            var buffer = new byte[1024];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            // Websocket will deliver a value in "CloseStatus" to indicate a closing.
            while (!result.CloseStatus.HasValue)
            {
                _logger.LogDebug("Wait For WebSocket Data");
                await stream.WriteAsync(buffer, 0, result.Count, CancellationToken.None);
                await stream.FlushAsync();
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _logger.LogDebug("Exited HandleWebSocketData");
        }

        /// <summary>
        /// Handles Incoming Data From StapServer.
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="stream"></param>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        private async Task HandleStapServerData(TcpClient tcpClient, NetworkStream stream, WebSocket webSocket)
        {
            _logger.LogDebug("Start HandleStapServerData");
            var stapServerBuffer = new byte[1024];

            while (tcpClient.Connected)
            {
                _logger.LogDebug("Wait For StapServer Data");
                var stapServerAnswer = await stream.ReadAsync(stapServerBuffer);
                await webSocket.SendAsync(
                    new ArraySegment<byte>(stapServerBuffer, 0, stapServerAnswer),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }

            _logger.LogDebug("Exited HandleStapServerData");
        }
    }
}