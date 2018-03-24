using System;
using System.Linq;
using Newtonsoft.Json;

namespace OmniSharp.Client.Commands
{
    public class WorkspaceInformationResponse
    {
        public WorkspaceInformationResponse(
            MSBuildSolution msBuildSolution)
        {
            MSBuildSolution = msBuildSolution;
        }

        [JsonProperty("MSBuild")]
        public MSBuildSolution MSBuildSolution { get; }
    }
}
