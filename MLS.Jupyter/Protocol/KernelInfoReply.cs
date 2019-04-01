using Newtonsoft.Json;

namespace MLS.Jupyter.Protocol
{
    public class KernelInfoReply
    {
        [JsonProperty("protocol_version")]
        public string ProtocolVersion { get; set; }

        [JsonProperty("implementation")]
        public string Implementation { get; set; }

        [JsonProperty("implementation_version")]
        public string ImplementationVersion { get; set; }

        [JsonProperty("language_info", NullValueHandling = NullValueHandling.Ignore)]
        public LanguageInfo LanguageInfo { get; set; }

        [JsonProperty("banner")]
        public string Banner { get; set; }
    }
}