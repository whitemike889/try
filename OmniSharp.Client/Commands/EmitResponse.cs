using System.Collections.Generic;

namespace OmniSharp.Client.Commands
{
    public class EmitResponse
    {
        public EmitResponse(string outputAssemblyPath, IReadOnlyCollection<Diagnostic> errors)
        {
            OutputAssemblyPath = outputAssemblyPath;
            Errors = errors;
        }

        public string OutputAssemblyPath { get; }

        public IReadOnlyCollection<Diagnostic> Errors { get; }
    }
}
