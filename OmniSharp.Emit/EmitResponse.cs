using System.Collections.Generic;
using MLS.Agent.Tools;

namespace OmniSharp.Emit
{
    public class EmitResponse
    {
        public string OutputAssemblyPath { get; set; }

        public IReadOnlyCollection<Diagnostic> Diagnostics { get; set; }
    }
}
