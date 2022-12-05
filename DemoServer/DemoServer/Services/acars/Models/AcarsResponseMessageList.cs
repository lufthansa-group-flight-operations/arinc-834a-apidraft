using System.Text.Json.Serialization;

namespace DemoServer.Models
{
    public class AcarsResponseMessageList
    {
        [JsonPropertyName("selfLink")]
        public string SelfLink { get; set; }

        [JsonPropertyName("limit")]
        public int? Limit { get; set; }

        [JsonPropertyName("start")]
        [JsonIgnore]
        public int Start { get; set; }

        [JsonPropertyName("total")]
        [JsonIgnore]
        public int? Total { get; set; }

        [JsonPropertyName("downlinks")]
        public AcarsDownlink[]? Downlinks { get; set; }

        [JsonPropertyName("uplinks")]
        public AcarsUplink[]? Uplinks { get; set; }
    }
}
