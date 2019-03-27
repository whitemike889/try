using Newtonsoft.Json;

namespace MLS.Jupyter.Protocol
{
    public class Link
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}