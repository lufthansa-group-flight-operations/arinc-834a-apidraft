using DemoServer.DataAccess;

namespace DemoServer.WebSockets
{
    public class WebSocketClientHandlerAcars : IWebSocketClientHandlerAcars
    {
        private readonly ILogger<WebSocketClientHandlerAcars> _logger;
        private readonly IAcarsMessageService _acars;

        public WebSocketClientHandlerAcars(ILogger<WebSocketClientHandlerAcars> logger, IAcarsMessageService acars)
        {

            _logger = logger;
            _acars = acars;
        }

        public void ReceiveDownlinkUpdate(object msg)
        {
            throw new NotImplementedException();
        }
    }
}
