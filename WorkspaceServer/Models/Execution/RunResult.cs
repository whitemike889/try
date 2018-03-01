using System;
using System.Collections.Generic;

namespace WorkspaceServer.Models.Execution
{
    public class RunResult
    {
        public RunResult(
            bool succeeded,
            IReadOnlyCollection<string> output = null,
            string exception = null,
            IReadOnlyCollection<SerializableDiagnostic> diagnostics = null)
        {
            Output = output ?? Array.Empty<string>();
            Succeeded = succeeded;
            Exception = exception;
            Diagnostics = diagnostics ?? Array.Empty<SerializableDiagnostic>();
        }

        public IReadOnlyCollection<SerializableDiagnostic> Diagnostics { get; set; }

        public bool Succeeded { get; }

        public IReadOnlyCollection<string> Output { get; }

        public string Exception { get; }

        public override string ToString() =>
            $@"{nameof(Succeeded)}: {Succeeded}
{nameof(Output)}: {string.Join("\n", Output)}
{nameof(Exception)}: {Exception}";
    }
}
