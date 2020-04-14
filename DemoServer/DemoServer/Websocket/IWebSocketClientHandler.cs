//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using DemoServer.Models;
using Microsoft.AspNetCore.Http;

namespace DemoServer.Websocket
{
    public interface IWebSocketClientHandler : IDisposable
    {
        void UpdateParameter(AvionicParameter parameter);
        Task StartListen(WebSocket webSocket, HttpContext httpContext);
    }
}