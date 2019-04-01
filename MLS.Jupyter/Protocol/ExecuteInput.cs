using Newtonsoft.Json;

namespace MLS.Jupyter.Protocol
{
    public class ExecuteInput
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("execution_count")]
        public int ExecutionCount { get; set; }
    }
}
