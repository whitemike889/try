using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.DotNet.Try.Jupyter.Protocol
{
    public class CompleteReply
    {
        [JsonProperty("matches")]
        public List<string> Matches { get; set; }

        [JsonProperty("cursor_start")]
        public int CursorStart { get; set; }

        [JsonProperty("cursor_end")]
        public int CursorEnd { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> MetaData { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
