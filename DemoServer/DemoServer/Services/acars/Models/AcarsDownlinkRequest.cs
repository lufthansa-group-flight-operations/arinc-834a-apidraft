using System.Text.Json.Serialization;

namespace DemoServer.Models
{
    public record AcarsDownlinkRequest : AcarsMessageBase
    {
        [JsonPropertyName("mediaSelect")]
        public AcarsMediaSelect MediaSelect { get; set; }
    }
}
