using Newtonsoft.Json;

namespace MLS.Jupyter.Protocol
{
    public class ExecuteReply
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("execution_count")]
        public int ExecutionCount { get; set; }
    }
}
