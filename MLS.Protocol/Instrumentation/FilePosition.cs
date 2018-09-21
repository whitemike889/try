using Newtonsoft.Json;
namespace MLS.Protocol.Instrumentation
{

    public class FilePosition
    {
        [JsonProperty("line")]
        public long Line { get; set; }

        [JsonProperty("character")]
        public long Character { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }
    }
}
