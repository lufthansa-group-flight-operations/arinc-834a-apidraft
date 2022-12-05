namespace DemoServer.Models
{
    public enum AcarsDownlinkState
    {
        queued,
        timeout,
        cmf_transfer,
        cmf_ack,
        dsp_ack,
        cmf_timeout,
        cmf_nak,
        deleted
    }
}