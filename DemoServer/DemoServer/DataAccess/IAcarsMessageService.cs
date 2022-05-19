using DemoServer.Models;

namespace DemoServer.DataAccess
{
    public interface IAcarsMessageService
    {
        AcarsStatus GetStatus();
        
        List<AcarsDownlink> Downlinks { get; set; }
        List<AcarsUplink> Uplinks { get; set; }

        void DeleteUplinks();
        void DeleteDownlinks();
        bool DeleteDownlink(int id);
        bool DeleteUplink(int id);
        AcarsDownlink? SendDownlink(AcarsDownlinkRequest request);
    }
}