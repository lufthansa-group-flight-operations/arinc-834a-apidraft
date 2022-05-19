namespace DemoServer.Models
{
    public enum AcarsDownlinkState
    {
        WAITING,
        TRANSMITTING,
        SENT,
        ACKNOWLEDGED,
        TIMED_OUT,
        FAILED
    }
}