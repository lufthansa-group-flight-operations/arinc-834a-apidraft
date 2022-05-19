using System.Text.Json.Serialization;

namespace DemoServer.Models
{
    public record AcarsDowlink
    {
        [JsonPropertyName("selfLink")]
        public string SelfLink { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("created")]
        public string Created { get; set; }

        [JsonPropertyName("state")]
        public AcarsDownlinkState State { get; set; }

        //public static explicit operator AcarsDowlink(AcarsDownlinkRequest v)
        //{
        //    return new AcarsDowlink();
        //}
    }
}