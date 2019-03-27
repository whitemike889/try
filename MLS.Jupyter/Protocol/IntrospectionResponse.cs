using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MLS.Jupyter.Protocol
{
    public class IntrospectionResponse
    {
        public static IntrospectionResponse Ok(string source, JObject data, JObject metaData) => new IntrospectionResponse{Status = StatusValues.Ok, Source = source, Data = data, MetaData = metaData};
        public static IntrospectionResponse Error(string source, JObject data, JObject metaData) => new IntrospectionResponse { Status = StatusValues.Error, Source = source, Data = data, MetaData = metaData };
        protected IntrospectionResponse()
        {
            Source = string.Empty;
            Data = new JObject();
            MetaData = new JObject();
        }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("data")]
        public JObject Data { get; set; }

        [JsonProperty("metadata")]
        public JObject MetaData { get; set; }
    }
}