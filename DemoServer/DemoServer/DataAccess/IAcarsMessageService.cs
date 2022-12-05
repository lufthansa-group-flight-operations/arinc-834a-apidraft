using DemoServer.Models;
using DemoServer.Services.acars;

namespace DemoServer.DataAccess
{
    public interface IAcarsMessageService
    {
        AcarsStatus GetStatus();
        
        List<AcarsDownlink> Downlinks { get; set; }
        List<AcarsUplink> Uplinks { get; set; }

        void DeleteUplinks();
        void DeleteDownlinks();
        bool DeleteDownlink(Guid id);
        bool DeleteUplink(Guid id);
        AcarsDownlink? SendDownlink(AcarsDownlinkRequest request);
        void Subscribe(IWebSocketClientHandlerAcars client);
        void Unsubscribe(IWebSocketClientHandlerAcars client);
    }
}