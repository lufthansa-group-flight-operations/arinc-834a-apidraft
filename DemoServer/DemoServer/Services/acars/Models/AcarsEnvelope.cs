using System.Text.Json.Serialization;

namespace DemoServer.Models
{
    public class AcarsEnvelope
    {
        public string type { get; set; }
        public object message { get; set; }
    }
}
