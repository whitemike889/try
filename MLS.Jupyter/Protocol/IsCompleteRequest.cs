using Newtonsoft.Json;

namespace MLS.Jupyter.Protocol
{
    public class IsCompleteRequest
    {
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}