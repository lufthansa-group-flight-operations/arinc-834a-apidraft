using System.Text.Json.Serialization;

namespace DemoServer.Models
{
    public class WebSocketAvionicParameterSubscription
    {
        [JsonPropertyName("method")]
        public string method { get; set; }

        [JsonPropertyName("arguments")]
        public SubscriptionArguments Arguments { get; set; }
    }

    public class SubscriptionArguments
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("interval")]
        public int? Interval { get; set; }

        [JsonPropertyName("parameters")]
        public string[] ParameterNames { get; set; }
    }
}
