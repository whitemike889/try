

using Newtonsoft.Json;

namespace Microsoft.DotNet.Try.Jupyter.Protocol
{
    public class Status
    {
        [JsonProperty("execution_state")]
        public string ExecutionState { get; set; }
    }
}
