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

        [JsonProperty("data")]
        public JObject Data { get; set; }

        [JsonProperty("metadata")]
        public JObject MetaData { get; set; }
    }

    public class UpdateDisplayData : DisplayData
    {
        public UpdateDisplayData()
        {
            Transient = new JObject();
        }

        [JsonProperty("transient")]
        public JObject Transient { get; set; }
    }
}
