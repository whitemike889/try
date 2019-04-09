using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.DotNet.Try.Protocol.Execution;
using Newtonsoft.Json;
using WorkspaceServer.Servers.Roslyn.Instrumentation;

namespace WorkspaceServer.Models.Instrumentation
{
    public class ProgramStateAtPositionArray : IRunResultFeature
    {
        [JsonProperty("instrumentation")]
        public IReadOnlyCollection<ProgramStateAtPosition> ProgramStates { get; set; }

        public string Name => nameof(ProgramStateAtPositionArray);

        public ProgramStateAtPositionArray(IReadOnlyCollection<string> programStates)
        {
            ProgramStates = programStates.Select(JsonConvert.DeserializeObject<ProgramStateAtPosition>).ToArray();
        }

        public void Apply(FeatureContainer result)
        {
            result.AddProperty("instrumentation", ProgramStates);
        }
    }
}

