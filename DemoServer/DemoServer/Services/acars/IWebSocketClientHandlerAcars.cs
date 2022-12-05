using System.Net.WebSockets;

namespace DemoServer.Services.acars
{
    public interface IWebSocketClientHandlerAcars
    {
        void ReceiveDownlinkUpdate(object msg);
        void Dispose();
        Task StartListen(WebSocket webSocket, HttpContext httpContext);
        void ReceiveUplinkUpdate(object msg);
    }
}
