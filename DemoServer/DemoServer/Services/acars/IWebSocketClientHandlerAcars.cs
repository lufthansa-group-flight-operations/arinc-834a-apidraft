using DemoServer.Models;
using System.Net.WebSockets;

namespace DemoServer.Services.acars
{
    public interface IWebSocketClientHandlerAcars
    {
        void ReceiveDownlinkUpdate(AcarsDownlink msg, bool includePayload = false);
        void Dispose();
        Task StartListen(WebSocket webSocket, HttpContext httpContext);
        void ReceiveUplinkUpdate(AcarsUplink msg, bool includePayload = false);
        void ReceiveStatusUpdate(object msg);
    }
}
