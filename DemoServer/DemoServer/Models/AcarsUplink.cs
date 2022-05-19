using System.Text;
using System.Text.Json.Serialization;

namespace DemoServer.Models
{
    public record AcarsUplink
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("timestamp")]
        public string TimeStamp { get; set; }

        [JsonPropertyName("mfi")]
        public string? Mfi { get; set; }

        [JsonPropertyName("lifetime")]
        public int? Lifetime { get; set; }

        [JsonPropertyName("dataType")]
        public AcarsDataType DataType { get; set; }

        [JsonPropertyName("dataSize")]
        public int DataSize { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }

        public string DataToString()
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(Data));             
        }

        /// <summary>
        /// Sets the Data field plain text and formats it into Base64;
        /// </summary>
        /// <param name="text">Text to set.</param>
        public void SetPlainTextToBas64(string text)
        {
            Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }
    }
}