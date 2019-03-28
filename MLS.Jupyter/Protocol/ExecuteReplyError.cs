using System.Collections.Generic;
using Newtonsoft.Json;

namespace MLS.Jupyter.Protocol
{
    public class ExecuteReplyError : ExecuteReply
    {
        public ExecuteReplyError()
        {
            Status = StatusValues.Error;
        }

        [JsonProperty("ename")]
        public string EName { get; set; }

        [JsonProperty("evalue")]
        public string EValue { get; set; }

        [JsonProperty("traceback")]
        public List<string> Traceback { get; set; } = new List<string>();
    }
}
