using System;
using System.Collections.Generic;
using System.Linq;
using MLS.Protocol.Execution;
using Newtonsoft.Json;

namespace MLS.Protocol.Instrumentation
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

