namespace DemoServer.Models
{
    public enum AcarsUplinkState
    {
        /// <summary>
        /// Uplink was received by the 834A server.
        /// </summary>
        received,

        /// <summary>
        /// Uplink was deleted on the 834A Server.
        /// </summary>
        deleted
    }
}
