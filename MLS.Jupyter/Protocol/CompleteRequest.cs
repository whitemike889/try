using Newtonsoft.Json;

namespace MLS.Jupyter.Protocol
{
    public class CompleteRequest
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("cursor_pos")]
        public int CursorPosition { get; set; }
    }
}
