using System.Collections.Generic;
using Newtonsoft.Json;

namespace MLS.Jupyter.Protocol
{
    public class ExecuteReplyOk : ExecuteReply
    {
        public ExecuteReplyOk()
        {
            this.Status = StatusValues.Ok;
        }

        [JsonProperty("payload")]
        public List<Dictionary<string,string>> Payload { get; set; }

        [JsonProperty("user_expressions")]
        public Dictionary<string,string> UserExpressions { get; set; }
    }
}
