using System;
using System.Collections.Generic;

namespace WorkspaceServer
{
    public class CommandLineResult
    {
        public CommandLineResult(
            int exitCode,
            IReadOnlyCollection<string> output = null,
            string exception = null)
        {
            ExitCode = exitCode;
            Output = output ?? Array.Empty<string>();
            Exception = exception;
        }

        public int ExitCode { get; }

        public IReadOnlyCollection<string> Output { get; }

        public string Exception { get; }
    }
}
