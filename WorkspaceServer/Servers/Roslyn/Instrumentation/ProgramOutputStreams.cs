using System;
using System.Collections.Generic;
using MLS.Protocol.Instrumentation;
using Newtonsoft.Json;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation
{
    public class ProgramOutputStreams
    {
        public ProgramOutputStreams(IReadOnlyCollection<string> stdOut, IReadOnlyCollection<string> instrumentation, string programDescriptor = "")
        {
            StdOut = stdOut ?? Array.Empty<string>();
            ProgramStatesArray = new ProgramStateAtPositionArray(instrumentation);
            ProgramDescriptor = JsonConvert.DeserializeObject<ProgramDescriptor>(programDescriptor);
        }

        public IReadOnlyCollection<string> StdOut { get; }

        public ProgramStateAtPositionArray ProgramStatesArray { get; }

        public ProgramDescriptor ProgramDescriptor { get; }
    }
}
