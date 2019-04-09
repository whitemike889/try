using Newtonsoft.Json;

namespace Microsoft.DotNet.Try.Jupyter.Protocol
{
    public class IsCompleteRequest
    {
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}