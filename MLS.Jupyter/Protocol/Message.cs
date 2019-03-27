using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MLS.Jupyter.Protocol
{
    public class Message
    {
        public Message()
        {
            Identifiers = new List<byte[]>();
            Signature = string.Empty;
            MetaData = new JObject();
            Content = new JObject();
            Buffers = new List<byte[]>();
        }

        [JsonIgnore]
        public List<byte[]> Identifiers { get; set; }

        [JsonIgnore]
        public string Signature { get; set; }

        [JsonProperty("header")]
        public Header Header { get; set; }

        [JsonProperty("parent_header")]
        public Header ParentHeader { get; set; }

        [JsonProperty("metadata")]
        public JObject MetaData { get; set; }

        [JsonProperty("content")]
        public JObject Content { get; set; }

        [JsonProperty("buffers")]
        public List<byte[]> Buffers { get; set; }
    }
}
