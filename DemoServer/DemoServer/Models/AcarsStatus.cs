using System.Text.Json.Serialization;

namespace DemoServer.Models
{
    public class AcarsStatus
    {
        [JsonPropertyName("selfLink")]
        public string Selflink { get; set; }

        [JsonPropertyName("updated")]
        public string  Updated { get; set; }

        [JsonPropertyName("isAnyAvailable")]
        public bool IsAnyAvailable { get; set; }

        [JsonPropertyName("channels")]
        public ChannelInfo[] Channels { get; set; }
    }

    public class ChannelInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("isAvailable")]
        public bool IsAvailable { get; set; }
    }
}
