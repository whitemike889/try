using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MLS.Jupyter.Protocol
{
    public class Message
    {
        [JsonIgnore]
        public List<byte[]> Identifiers { get; set; } = new List<byte[]>();

        [JsonIgnore]
        public string Signature { get; set; } = string.Empty;

        [JsonProperty("header")]
        public Header Header { get; set; }

        [JsonProperty("parent_header")]
        public Header ParentHeader { get; set; }

        [JsonProperty("metadata")]
        public object MetaData { get; set; } = new object();

        [JsonProperty("content")]
        public object Content { get; set; } = new object();

        [JsonProperty("buffers")]
        public List<byte[]> Buffers { get; } = new List<byte[]>();
    }
}