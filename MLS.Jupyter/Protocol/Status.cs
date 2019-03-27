

using Newtonsoft.Json;

namespace MLS.Jupyter.Protocol
{
    public class Status
    {
        [JsonProperty("execution_state")]
        public string ExecutionState { get; set; }
    }
}
