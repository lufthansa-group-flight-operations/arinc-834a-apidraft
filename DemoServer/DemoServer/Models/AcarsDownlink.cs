using System.Text.Json.Serialization;

namespace DemoServer.Models
{
    public record AcarsDownlink : AcarsDownlinkRequest
    {

        [JsonPropertyName("created")]
        public string Created { get; set; }

        [JsonPropertyName("state")]
        public AcarsDownlinkState State { get; set; }

        [JsonPropertyName("selfLink")]
        public string SelfLink { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        //public AcarsDownlink()
        //{
        //    //Intentionally left blank
        //}

        public AcarsDownlink(AcarsDownlinkRequest request)
        {
            this.SelfLink = request.SelfLink;
            this.Id = request.Id;
            this.Mfi = request.Mfi;
            this.DataType = request.DataType;
            this.Data = request.Data;
            this.DataSize = request.DataSize;
            this.LifeTime = request.LifeTime;
            this.MediaSelect = request.MediaSelect;
        }
    }

    public record AcarsDownlinkWithoutData : AcarsDownlink
    {
        protected AcarsDownlinkWithoutData(AcarsDownlink original) : base(original)
        {
        }

        [JsonIgnore]
        public new string? Data { get; set; }
    }
}