//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DemoServer.DataAccess;
using DemoServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace DemoServer.Websocket
{
    public class WebSocketClientHandler : IWebSocketClientHandler
    {
        private readonly ILogger _logger;
        private readonly IAvionicDataSource _avionicData;
        private readonly IConfiguration configuration;
        private readonly Timer _timerTimeOut;

        private HashSet<string> _eventFilter;
        private ConcurrentDictionary<string, AvionicParameter> _parameterBuffer;
        private ConcurrentDictionary<string, object> _parameterBufferLastValue;
        private bool _receivedValidSupscription = false;
        private WebSocketAvionicsParameterSubscriptionType? _subscriptionType;
        private Timer _timerContinousDelivery;
        private WebSocket _socket;
        private HttpContext _context;
        private CancellationTokenSource _tokenSource;
        private JsonSerializerOptions _serializerOption;

        public WebSocketClientHandler(ILogger<WebSocketClientHandler> logger, IAvionicDataSource avionicData)
        {
            _logger = logger;            
            _avionicData = avionicData;
            _serializerOption = new JsonSerializerOptions() 
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };
            
            // Set and Start Timeout Timer for request
            _timerTimeOut = new Timer(10000);            
            _timerTimeOut.Elapsed += TimeOutElapsed;      
            _timerTimeOut.Start();

            // Init the timer for now.
            _timerContinousDelivery = new Timer();
            _timerContinousDelivery.Elapsed += _timerContinousDeliveryElapsed;

            _eventFilter = new HashSet<string>();
        }

        /// <inheritdoc />
        public async Task StartListen(WebSocket webSocket, HttpContext httpContext)
        {
            _socket = webSocket;
            _context = httpContext;

            _tokenSource = new CancellationTokenSource();
            _avionicData.Subscribe(this);

            // All Stuff taking place here.
            await StartEventLoop();

            _timerContinousDelivery?.Stop();
            _avionicData.Unsubscribe(this);
        }

        private async Task StartEventLoop()
        {
            var buffer = new byte[1 << 16]; // 64k

            _logger.LogDebug("Start listening.");
            while (_socket.State == WebSocketState.Open)
            {
                var rc = await _socket.ReceiveAsync(buffer, _tokenSource?.Token ?? CancellationToken.None);
                if (rc.EndOfMessage)
                {
                    var msg = string.Empty;
                    switch (rc.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            try
                            {
                                msg = Encoding.UTF8.GetString(buffer, 0, rc.Count);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError("Received Non-UTF-8 data, closing WebSocket");
                                await CloseWebSocketAsync(WebSocketSubscriptionErrorCode.BadFormat);
                                return;
                            }

                            await HandleMessage(msg);
                            break;
                        case WebSocketMessageType.Binary:
                            msg = BitConverter.ToString(buffer).Replace('-', ':');
                            break;
                        case WebSocketMessageType.Close:
                            _logger.LogDebug("Websocket close received");
                            return;
                    }

                    var ip = _context.Connection.RemoteIpAddress;
                    _logger.LogInformation($"Message [{msg}] received from [{ip}]");
                }
            }
            _logger.LogDebug("Websocket is closed");
        }

        private async Task HandleMessage(string msg)
        {
            try
            {
                var subscribeRequest = JsonSerializer.Deserialize<WebSocketAvionicParameterSubscription>(msg, _serializerOption);
                _logger.LogDebug("Received Valid Json");
                HandleSubscription(subscribeRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError("Invalid JSON Request received.");
                SendError(WebSocketSubscriptionErrorCode.BadFormat);
                await CloseWebSocketAsync(WebSocketSubscriptionErrorCode.BadFormat);
            }
        }

        private async void HandleSubscription(WebSocketAvionicParameterSubscription subscribeRequest)
        {
            // Check method of request, and inform client and close connection, if its not a Subscription for parameters
            if (subscribeRequest.method.ToLower() != "SubscribeParameters".ToLower())
            {
                _logger.LogError($"Error in JSON: Invalid Method in JSON, Expected: 'SubscribeParameter', Receveid: {subscribeRequest.method}");
                SendError(WebSocketSubscriptionErrorCode.BadRequest);
                await CloseWebSocketAsync(WebSocketSubscriptionErrorCode.BadRequest);
                return;
            }

            // No Parameters given in the Request
            if (subscribeRequest.Arguments?.ParameterNames == null)
            {
                _logger.LogError($"Error in JSON: ParameterNames are null");
                SendError(WebSocketSubscriptionErrorCode.BadRequest);
                await CloseWebSocketAsync(WebSocketSubscriptionErrorCode.BadRequest);
                return;
            }

            // No Parameters given in the Request
            if (subscribeRequest.Arguments.ParameterNames.Length == 0)
            {
                _logger.LogError($"Error in JSON: ParameterNames are empty");
                SendError(WebSocketSubscriptionErrorCode.BadRequest);
                await CloseWebSocketAsync(WebSocketSubscriptionErrorCode.BadRequest);
                return;
            }

            var (known, unknown) = await CheckParamters(subscribeRequest.Arguments.ParameterNames);

            // If no known paramters found, return error and close connection
            if (known.Length == 0)
            {
                SendError(WebSocketSubscriptionErrorCode.AllunknownParameters);
                await CloseWebSocketAsync(WebSocketSubscriptionErrorCode.AllunknownParameters);
                return;
            }

            // Otherwise, check what subscripton method is requested
            switch (subscribeRequest.Arguments.Type.ToLower())
            {
                case "on change":
                    _subscriptionType = WebSocketAvionicsParameterSubscriptionType.OnChange;
                    break;
                case "on update":
                    _subscriptionType = WebSocketAvionicsParameterSubscriptionType.OnUpdate;
                    break;
                case "continuous":
                    _subscriptionType = WebSocketAvionicsParameterSubscriptionType.Continuous;
                    if (subscribeRequest.Arguments.Interval is null)
                    {
                        SendError(WebSocketSubscriptionErrorCode.InvalidPeriod);
                        await CloseWebSocketAsync(WebSocketSubscriptionErrorCode.InvalidPeriod);
                        return;
                    }
                    _timerContinousDelivery.Interval = (double)subscribeRequest.Arguments.Interval;
                    _timerContinousDelivery.Start();
                    _logger.LogDebug($"Starte timer for continuous dilivery with interval: {subscribeRequest.Arguments.Interval}");
                    break;
                default:
                    // Anythin else would be not valid
                    SendError(WebSocketSubscriptionErrorCode.BadRequest);
                    await CloseWebSocketAsync(WebSocketSubscriptionErrorCode.BadRequest);
                    return;
            }

            // Seems we have a valid subscription, stop the timout-timer.
            _receivedValidSupscription = true;
            _timerTimeOut.Stop();

            // Now signal that to the client
            await SendOk(unknown);

            switch (_subscriptionType)
            {
                case WebSocketAvionicsParameterSubscriptionType.Continuous:
                    // No need to send all current values of subscribed params.
                    // But we need a buffer here, so init the buffer
                    _parameterBuffer = new ConcurrentDictionary<string, AvionicParameter>();
                    foreach (var item in known)
                    {
                        _parameterBuffer.TryAdd(item.Name, item);
                    }
                    break;
                case WebSocketAvionicsParameterSubscriptionType.OnChange:
                    //Send all current values
                    SendData(new AvionicParameters() { Parameters = known });
                    // Init Buffer
                    _parameterBuffer = new ConcurrentDictionary<string, AvionicParameter>();
                    foreach (var item in known)
                    {
                        _parameterBuffer.TryAdd(item.Name, item);
                    }
                    _parameterBufferLastValue = new ConcurrentDictionary<string, object>();
                    foreach (var item in known)
                    {
                        _parameterBufferLastValue.TryAdd(item.Name, item.Value);
                    }
                    break;
                case WebSocketAvionicsParameterSubscriptionType.OnUpdate:
                    //Send all current values
                    SendData(new AvionicParameters() { Parameters = known });
                    // No need to init Buffer.
                    break;
                default:
                    _logger.LogError("Invalid Subsciption type set");
                    break;
            }

            // Add the known parameters to our incoming data filter
            _eventFilter.UnionWith(known.Select(p => p.Name).ToArray());
        }

        private async Task<(AvionicParameter[] knownParams, string[] unknownParams)> CheckParamters(string[] parameterNames)
        {
            // Assure parameterNames are only once 
            var uniqueRequest = new HashSet<string>();
            uniqueRequest.UnionWith(parameterNames);

            // Detect known and unknowm parameters
            var foundParameters = _avionicData.GetParameters().Where(p => uniqueRequest.Contains(p.Name)).ToArray();
            var notFoundParameters = uniqueRequest.Where(u => !foundParameters.Select(p => p.Name).Contains(u)).ToArray();
            new AvionicParameters();

            return (foundParameters, notFoundParameters);
        }

        /// <summary>
        /// Fired when TimeOut Timer has elapsed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeOutElapsed(object? sender, ElapsedEventArgs e)
        {
            // If no valid subscription is received until now, inform client and close WS.
            if (!_receivedValidSupscription)
            {
                _logger.LogWarning("Timeout: No subscription request received within ");
                _timerTimeOut.Stop();
                SendError(WebSocketSubscriptionErrorCode.TimeOut);
                CloseWebSocketAsync(WebSocketSubscriptionErrorCode.TimeOut).Wait();
            }
        }
        
        private void _timerContinousDeliveryElapsed(object? sender, ElapsedEventArgs e)
        {
            SendData(new AvionicParameters() { Parameters = _parameterBuffer.Values.ToArray() });
        }

        /// <summary>
        /// Closes WebSocket connection, using provided reason code.
        /// </summary>
        /// <param name="reason">Reason Code to send along the WS close.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Sends an ok response to the client, and adds unknown paramaters if given.
        /// </summary>
        /// <param name="unknownParameters">Optional: Unknown parameter in the request.</param>
        /// <returns></returns>
        private async Task SendOk(string[]? unknownParameters)
        {
            var response = new WebSocketResponse() 
            { 
                ReturnCode = "Ok"
            };

            if (unknownParameters?.Length > 0)
            {
                response.UnKnownParameters = unknownParameters;
            }

            SendData(response);            
        }

        /// <summary>
        /// Sends an error response with the given error reason.
        /// </summary>
        /// <param name="reason">Reason enum </param>
        private void SendError(WebSocketSubscriptionErrorCode reason)
        {
            var errorResponse = new WebSocketResponse()
            {
                ReturnCode = WebSocketResponseCode.Error.ToString(),
                Reason = reason.ToString()
            };
            SendData(JsonSerializer.Serialize(errorResponse, _serializerOption));
        }

        private void SendOnTimer(object sender, ElapsedEventArgs e)
        {
            var data = _avionicData.GetParameters();
            if (_eventFilter.Count > 0)
            {
                data = data.Where(p => _eventFilter.Contains(p.Name)).ToArray();
            }

            var jsonResult = JsonSerializer.Serialize(new AvionicParameters { Parameters = data }, _serializerOption);
            SendData(jsonResult);
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
                }).ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        _logger.LogError("Send failed.", t.Exception);
                        _tokenSource?.Cancel();
                        _timerContinousDelivery.Stop();
                    }
                }).ConfigureAwait(false);
        }

        /// <inheritdoc />
        /// This Method is called from inside the _avionicData
        public void UpdateParameter(AvionicParameter data)
        {
            if (data == null || !_eventFilter.Contains(data.Name))
            {
                // If a parameter is not in our list, drop it
                return;
            }

            switch (_subscriptionType)
            {
                case WebSocketAvionicsParameterSubscriptionType.Continuous:
                    if (_parameterBuffer.ContainsKey(data.Name))
                    {
                        _parameterBuffer[data.Name] = data;
                    }
                    else
                    {
                        _parameterBuffer.TryAdd(data.Name, data);
                    }
                    break;
                case WebSocketAvionicsParameterSubscriptionType.OnChange:
                    if (!_parameterBuffer.ContainsKey(data.Name))
                    {
                        _parameterBuffer.TryAdd(data.Name, data);
                        SendData(new AvionicParameters() { Parameters = new AvionicParameter[] { data } });
                        break;
                    }

                    // To compare, we need to stringalize it, to make it compareable
                    if (!string.Equals(_parameterBuffer[data.Name].Value?.ToString(),data.Value?.ToString()))
                    {
                        _parameterBuffer[data.Name] = data;
                        SendData(new AvionicParameters() { Parameters = new AvionicParameter[] { data } });
                    }

                    //if (_parameterBufferLastValue.ContainsKey(data.Name))
                    //{
                    //    // Id value differs from previously stored value, store it
                    //    // and send it to the client.
                    //    if (_parameterBufferLastValue[data.Name] != data.Value)
                    //    {
                    //        _parameterBufferLastValue[data.Name] = data.Value;
                    //        SendData(new AvionicParameters() { Parameters = new AvionicParameter[] { data } });
                    //    }
                    //}
                    //else
                    //{
                    //    _parameterBufferLastValue.TryAdd(data.Name, data.Value);
                    //}
                    break;
                case WebSocketAvionicsParameterSubscriptionType.OnUpdate:
                    SendData(new AvionicParameters() { Parameters = new AvionicParameter[] { data } });
                    break;
                default:
                    break;
            }            
        }

        public void Dispose()
        {
            _timerContinousDelivery?.Dispose();
            _timerTimeOut?.Dispose();
            _socket?.Dispose();
            _tokenSource?.Dispose();
        }
    }

    public enum WebSocketAvionicsParameterSubscriptionType
    {
        /// <summary>Sends continously data by client specified interval.</summary>        
        Continuous,
        /// <summary>Sends data to client, if value has changed.</summary>
        OnChange,
        /// <summary>Sends data to client, as soon as received from avionics.</summary>
        OnUpdate
    }
}