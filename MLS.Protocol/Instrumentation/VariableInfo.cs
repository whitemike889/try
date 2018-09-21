using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MLS.Protocol.Instrumentation
{
    public class VariableInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public JToken Value { get; set; }

        [JsonProperty("declaredAt")]
        public LineRange RangeOfLines { get; set; }
    }
}
