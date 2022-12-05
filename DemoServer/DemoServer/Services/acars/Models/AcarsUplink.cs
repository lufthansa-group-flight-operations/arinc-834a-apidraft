using System.Text;
using System.Text.Json.Serialization;

namespace DemoServer.Models
{
    /// <summary>
    /// Acars Uplink message.
    /// </summary>
    public record AcarsUplink : AcarsMessageBase
    {
        /// <summary>
        /// Indicates the state of the uplink message.
        /// </summary>
        [JsonPropertyName("state")]
        public AcarsUplinkState State { get; set; }

        public string DataToString()
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(Payload));             
        }
    }
}