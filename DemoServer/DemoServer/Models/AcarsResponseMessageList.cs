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
        public int Start { get; set; }

        [JsonPropertyName("total")]
        public int? Total { get; set; }

        [JsonPropertyName("downlinks")]
        public AcarsDownlink[]? Downlinks { get; set; }

        [JsonPropertyName("uplinks")]
        public AcarsUplink[]? Uplinks { get; set; }
    }
}
