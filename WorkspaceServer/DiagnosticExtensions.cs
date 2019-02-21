using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using MLS.Protocol.Diagnostics;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

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
