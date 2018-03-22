using System.Collections.Generic;

namespace OmniSharp.Client.Commands
{
    public class EmitResponse
    {
        public EmitResponse(string outputAssemblyPath, IReadOnlyCollection<Diagnostic> diagnostics)
        {
            OutputAssemblyPath = outputAssemblyPath;
            Diagnostics = diagnostics;
        }

        public string OutputAssemblyPath { get; }

        public IReadOnlyCollection<Diagnostic> Diagnostics { get; }
    }
}
