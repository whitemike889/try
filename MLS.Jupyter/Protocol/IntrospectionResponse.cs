using Newtonsoft.Json;

namespace MLS.Jupyter.Protocol
{
    public class IntrospectionResponse
    {
        public static IntrospectionResponse Ok(string source, object data, object metaData) => new IntrospectionResponse{Status = StatusValues.Ok, Source = source, Data = data, MetaData = metaData};
        public static IntrospectionResponse Error(string source, object data, object metaData) => new IntrospectionResponse { Status = StatusValues.Error, Source = source, Data = data, MetaData = metaData };
        protected IntrospectionResponse()
        {
            Source = string.Empty;
            Data = new object();
            MetaData = new object();
        }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public object Data { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public object MetaData { get; set; }
    }
}