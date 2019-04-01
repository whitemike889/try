using Newtonsoft.Json;

namespace MLS.Jupyter.Protocol
{
    public class ShutdownRequestReply
    {
        [JsonProperty("restart")]
        public bool Restart { get; set; }
    }
}
