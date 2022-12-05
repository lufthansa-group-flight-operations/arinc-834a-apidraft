using Microsoft.AspNetCore.Server.IIS.Core;
using System.Text.Json.Serialization;

namespace DemoServer.Models
{
    public class AcarsEnvelope
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("message")]
        public object Message { get; set; }

        public AcarsEnvelope()
        {
            //Intionally left blank
        }

        public AcarsEnvelope(string type, object message)
        {
            Type = type;
            Message = message;
        }
    }
}
