using Newtonsoft.Json;

namespace Microsoft.DotNet.Try.Client.Configuration
{
    public class RequestDescriptors
    {
        [JsonProperty("_self")]
        public RequestDescriptor Self { get; }
        public RequestDescriptor Configuration { get; set; }
        public RequestDescriptor Completion { get; set; }
        public RequestDescriptor AcceptCompletion { get; set; }
        public RequestDescriptor LoadFromGist { get; set; }
        public RequestDescriptor Diagnostics { get; set; }
        public RequestDescriptor SignatureHelp { get; set; }
        public RequestDescriptor Run { get; set; }
        public RequestDescriptor Snippet { get; set; }
        public RequestDescriptor Version { get; set; }
        public RequestDescriptor Compile { get; set; }
        public RequestDescriptor ProjectFromGist { get; set; }
        public RequestDescriptor RegionsFromFiles { get; set; }

        public RequestDescriptors(RequestDescriptor self)
        {
            Self = self;
        }
    }
}