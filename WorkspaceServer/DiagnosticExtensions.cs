using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace WorkspaceServer
{
    public static class DiagnosticExtensions
    {
        public static bool IsError(this Diagnostic diagnostic)
        {
            return diagnostic.Severity == DiagnosticSeverity.Error;
        }

        public static bool ContainsError(this IEnumerable<Diagnostic> diagnostics)
        {
            return diagnostics.Any(e => e.IsError());
        }
    }
}
