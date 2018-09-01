using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Roslyn.Instrumentation;

namespace WorkspaceServer.Models.Instrumentation
{
    public class ProgramStateAtPositionArray : IRunResultFeature
    {
        [JsonProperty("instrumentation")]
        public IReadOnlyCollection<ProgramStateAtPosition> ProgramStates { get; set; }

        public ProgramStateAtPositionArray(IReadOnlyCollection<string> programStates)
        {
            ProgramStates = programStates.Select(JsonConvert.DeserializeObject<ProgramStateAtPosition>).ToArray();
        }

        public void Apply(RunResult result)
        {
            result.AddProperty("instrumentation", ProgramStates);
        }
    }
}

