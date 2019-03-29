using Newtonsoft.Json;

namespace MLS.Jupyter.Protocol
{
    public class ExecuteResult : DisplayData
    {
        [JsonProperty("execution_count", NullValueHandling = NullValueHandling.Ignore)]
        public int ExecutionCount { get; set; }
    }
}