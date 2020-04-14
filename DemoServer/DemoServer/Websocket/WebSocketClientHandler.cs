//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
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
        private readonly Timer _timer;
        private readonly HashSet<string> _eventFilter;

        private WebSocket _socket;
        private HttpContext _context;
        private CancellationTokenSource _tokenSource;

        public WebSocketClientHandler(ILogger<WebSocketClientHandler> logger, IAvionicDataSource avionicData)
        {
            _logger = logger;
            _avionicData = avionicData;

            _timer = new Timer();
            _timer.Elapsed += SendOnTimer;
            _eventFilter = new HashSet<string>();
        }

        /// <inheritdoc />
        public async Task StartListen(WebSocket webSocket, HttpContext httpContext)
        {
            _socket = webSocket;
            _context = httpContext;

            _tokenSource = new CancellationTokenSource();
            var req = _context.Request.Query;

            // Build parameter filter.
            if (req.ContainsKey("keys"))
            {
                var keys = req["keys"].First().Split(",");
                foreach (var key in keys)
                {
                    _eventFilter.Add(key);
                }
            }

            if (req.ContainsKey("method") && req["method"].ToString().Equals("event", StringComparison.OrdinalIgnoreCase))
            {
                _avionicData.Subscribe(this);
            }
            else
            {
                var interval = 1000;
                if (req.ContainsKey("interval"))
                {
                    int.TryParse(req["interval"], out interval);
                    if (interval < 50 || interval > 10000)
                    {
                        interval = 1000;
                    }
                }

                _timer.Interval = interval;
                _timer.Start();
            }

            await StartEventLoop();

            _timer.Stop();
            _avionicData.Unsubscribe(this);
        }

        /// <inheritdoc />
        public void UpdateParameter(AvionicParameter data)
        {
            if (data == null || !_eventFilter.Contains(data.Key))
            {
                return;
            }

            var jsonResult = JsonSerializer.Serialize(data);
            SendData(jsonResult);
        }

        private void SendOnTimer(object sender, ElapsedEventArgs e)
        {
            var data = _avionicData.GetParameters();
            if (_eventFilter.Count > 0)
            {
                data = data.Where(p => _eventFilter.Contains(p.Key)).ToArray();
            }

            var jsonResult = JsonSerializer.Serialize(new AvionicParameters { Parameters = data });
            SendData(jsonResult);
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
                            msg = Encoding.UTF8.GetString(buffer);
                            break;
                        case WebSocketMessageType.Binary:
                            msg = BitConverter.ToString(buffer).Replace('-', ':');
                            break;
                        case WebSocketMessageType.Close:
                            return;
                    }

                    var ip = _context.Connection.RemoteIpAddress;
                    _logger.LogInformation($"Message [{msg}] received from [{ip}]");
                }
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
                        _timer.Stop();
                    }
                }).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _socket?.Dispose();
            _tokenSource?.Dispose();
        }
    }
}