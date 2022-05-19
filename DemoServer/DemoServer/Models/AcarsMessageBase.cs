using System.Text;
using System.Text.Json.Serialization;

namespace DemoServer.Models
{
    public record AcarsMessageBase
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("selfLink")]
        public string SelfLink { get; set; }

        [JsonPropertyName("mfi")]
        public string? Mfi { get; set; }

        [JsonPropertyName("mediaSelect")]
        public AcarsMediaSelect MediaSelect { get; set; }

        [JsonPropertyName("lifetime")]
        public int LifeTime { get; set; }

        [JsonPropertyName("dataType")]
        public AcarsDataType DataType { get; set; }

        [JsonPropertyName("dataSize")]
        public int DataSize { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }

        public string ToBase64String()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Data));
        }

        internal bool Validate()
        {
            // Put validation in here
            return true;
        }

        public bool ShouldSerializeData()
        {
            return false;
        }
    }
}
