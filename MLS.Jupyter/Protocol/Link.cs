using Newtonsoft.Json;

namespace Microsoft.DotNet.Try.Jupyter.Protocol
{
    public class Link
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}