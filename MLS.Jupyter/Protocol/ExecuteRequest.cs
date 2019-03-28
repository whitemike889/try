using System.Collections.Generic;
using Newtonsoft.Json;

namespace MLS.Jupyter.Protocol
{
    public class ExecuteRequest
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("silent")]
        public bool Silent { get; set; } = false;

        [JsonProperty("store_history")]
        public bool StoreHistory { get; set; } = false;

        [JsonProperty("user_expressions")]
        public Dictionary<string,string> UserExpressions { get; set; }

        [JsonProperty("allow_stdin")]
        public bool AllowStdin { get; set; } = true;

        [JsonProperty("stop_on_error")]
        public bool StopOnError { get; set; } = false;
    }
}
