using Newtonsoft.Json;

namespace Microsoft.DotNet.Try.Jupyter.Protocol
{
    public class ShutdownRequestReply
    {
        [JsonProperty("restart")]
        public bool Restart { get; set; }
    }
}
