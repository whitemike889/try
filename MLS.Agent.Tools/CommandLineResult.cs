using System;
using System.Collections.Generic;

namespace MLS.Agent.Tools
{
    public class CommandLineResult
    {
        public CommandLineResult(
            int exitCode,
            IReadOnlyCollection<string> output = null,
            IReadOnlyCollection<string> error = null)
        {
            ExitCode = exitCode;
            Output = output ?? Array.Empty<string>();
            Error = error ?? Array.Empty<string>();
        }

        public int ExitCode { get; }

        public IReadOnlyCollection<string> Output { get; }

        public IReadOnlyCollection<string> Error { get; }
    }
}
