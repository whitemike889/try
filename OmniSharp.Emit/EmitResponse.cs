using System.Collections.Generic;

namespace OmniSharp.Emit
{
    public class EmitResponse
    {
        public string OutputAssemblyPath { get; set; }

        public IReadOnlyCollection<Diagnostic> Errors { get; set; }
    }
}
