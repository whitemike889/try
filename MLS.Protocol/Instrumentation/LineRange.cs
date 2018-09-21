using Newtonsoft.Json;

namespace MLS.Protocol.Instrumentation
{
    public class LineRange
    {
        [JsonProperty("start")]
        public long Start { get; set; }

        [JsonProperty("end")]
        public long End { get; set; }
    }
}
