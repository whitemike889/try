using Newtonsoft.Json;

namespace Microsoft.DotNet.Try.Jupyter.Protocol
{
    public class CompleteRequest
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("cursor_pos")]
        public int CursorPosition { get; set; }
    }
}
