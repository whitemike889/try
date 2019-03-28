using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MLS.Jupyter.Protocol
{
    public class DisplayData
    {
        public DisplayData()
        {
            Source = string.Empty;
            Data = new JObject();
            MetaData = new JObject();
        }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public object Data { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public object MetaData { get; set; }
    }

    public class UpdateDisplayData : DisplayData
    {
        public UpdateDisplayData()
        {
            Transient = new JObject();
        }

        [JsonProperty("transient", NullValueHandling = NullValueHandling.Ignore)]
        public object Transient { get; set; }
    }
}
