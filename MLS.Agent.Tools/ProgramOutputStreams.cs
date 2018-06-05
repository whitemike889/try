using System;
using System.Collections.Generic;
using System.Text;

namespace MLS.Agent.Tools
{
    public class ProgramOutputStreams
    {
        public ProgramOutputStreams(IReadOnlyCollection<string> stdOut, IReadOnlyCollection<string> instrumentation)
        {
            StdOut = stdOut ?? Array.Empty<string>();
            Instrumentation = instrumentation ?? Array.Empty<string>();
        }

        public IReadOnlyCollection<string> StdOut { get; }

        public IReadOnlyCollection<string> Instrumentation { get; }
    }
}
