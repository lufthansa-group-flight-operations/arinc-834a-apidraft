using System.Text.Json.Serialization;

namespace DemoServer.Models
{    
    public record AcarsDownlink : AcarsMessageBase
    {
        // Other Properties are inherited from "AcarsMessageBase"

        [JsonPropertyName("mediaSelect")]
        public AcarsMediaSelect MediaSelect { get; set; }

        [JsonPropertyName("state")]
        public AcarsDownlinkState State { get; set; }

        [JsonPropertyName("status_update_timestamp")]
        public DateTime StatusUpateTimeStamp { get; set; }

        public AcarsDownlink()
        {
            // Intntionally Left Blank
        }

        public AcarsDownlink(AcarsDownlinkRequest request)
        {
            this.SelfLink = request.SelfLink;
            this.Id = request.Id;
            this.Mti = request.Mti;
            this.DataType = request.DataType;
            this.Payload = request.Payload;
            this.DataSize = this.Payload.Length;
            this.LifeTime = request.LifeTime;
            this.MediaSelect = request.MediaSelect;
        }
    }
}