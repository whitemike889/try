using Newtonsoft.Json;

namespace Microsoft.DotNet.Try.Protocol.ClientApi
{
    public class CreateProjectRequest : MessageBase
    {
        [JsonProperty("projectTemplate")]
        public string ProjectTemplate { get; }

        public CreateProjectRequest(string requestId, string projectTemplate) : base(requestId)
        {
            ProjectTemplate = projectTemplate;
        }
    }
}