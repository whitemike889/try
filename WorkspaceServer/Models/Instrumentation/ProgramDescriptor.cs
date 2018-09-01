using Newtonsoft.Json;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Roslyn.Instrumentation;

namespace WorkspaceServer.Models.Instrumentation
{
    public class ProgramDescriptor : IRunResultFeature
    {
        [JsonProperty("variableLocations")]
        public VariableLocation[] VariableLocations { get; set; }

        public void Apply(RunResult result)
        {
            result.AddProperty("variableLocations", VariableLocations);
        }
    }

    public class VariableLocation
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("locations")]
        public Location[] Locations { get; set; }

        [JsonProperty("declaredAt")]
        public RangeOfLines RangeOfLines { get; set; }
    }

    public class Location
    {
        [JsonProperty("startLine")]
        public long StartLine { get; set; }

        [JsonProperty("startColumn")]
        public long StartColumn { get; set; }

        [JsonProperty("endLine")]
        public long EndLine { get; set; }

        [JsonProperty("endColumn")]
        public long EndColumn { get; set; }
    }
}
