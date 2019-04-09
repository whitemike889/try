using Newtonsoft.Json;

namespace Microsoft.DotNet.Try.Protocol.ClientApi
{
    public class CreateProjectResponse : MessageBase
    {
        [JsonProperty("project")]
        public Project Project { get; }

        public CreateProjectResponse(string requestId, Project project) : base(requestId)
        {
            Project = project;
        }
    }
}