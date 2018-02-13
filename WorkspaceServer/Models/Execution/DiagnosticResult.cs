using System;
using System.Collections.Generic;
using System.Text;

namespace WorkspaceServer.Models.Execution
{
    public class DiagnosticResult
    {
        public DiagnosticResult(IReadOnlyCollection<SerializableDiagnostic> diagnostics)
        {
            Diagnostics = diagnostics ?? Array.Empty<SerializableDiagnostic>();
        }

        public IReadOnlyCollection<SerializableDiagnostic> Diagnostics { get; set; }
    }
}
