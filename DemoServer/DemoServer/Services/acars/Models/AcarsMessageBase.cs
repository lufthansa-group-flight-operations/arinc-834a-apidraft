using System.Text;
using System.Text.Json.Serialization;

namespace DemoServer.Models
{
    public record AcarsMessageBase
    {
        //[JsonPropertyName("id")]
        [JsonIgnore]
        public Guid Id { get; set; }

        [JsonPropertyName("selfLink")]
        public string? SelfLink { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime TimeStamp { get; set; }

        [JsonPropertyName("mti")]
        public string? Mti { get; set; }

        //[JsonPropertyName("mediaSelect")]
        //public AcarsMediaSelect MediaSelect { get; set; }

        [JsonPropertyName("lifetime")]
        public int LifeTime { get; set; }

        [JsonPropertyName("dataType")]
        public AcarsDataType DataType { get; set; }

        [JsonPropertyName("dataSize")]
        public int DataSize { get; set; }

        [JsonPropertyName("payload")]
        public string Payload { get; set; }
        //public string Payload { get; set; }

        public string ToBase64String()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Payload));
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
