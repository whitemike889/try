using Newtonsoft.Json;

namespace Microsoft.DotNet.Try.Jupyter.Protocol
{
    public class ExecuteResult : DisplayData
    {
        [JsonProperty("execution_count", NullValueHandling = NullValueHandling.Ignore)]
        public int ExecutionCount { get; set; }
    }
}