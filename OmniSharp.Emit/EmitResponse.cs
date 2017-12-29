using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace OmniSharp.Emit
{
    public class EmitResponse
    {
        public string OutputAssemblyPath { get; set; }

        public IReadOnlyCollection<Diagnostic> Errors { get; set; }
    }
}
