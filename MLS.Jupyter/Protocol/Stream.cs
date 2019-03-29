using Newtonsoft.Json;

namespace MLS.Jupyter.Protocol
{
    public class Stream
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }

    }
}