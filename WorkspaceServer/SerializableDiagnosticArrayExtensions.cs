using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.DotNet.Try.Protocol.Diagnostics;

namespace WorkspaceServer
{
    public static class SerializableDiagnosticArrayExtensions
    {
        public static bool ContainsError(this IEnumerable<SerializableDiagnostic> diagnostics)
        {
            return diagnostics.Any(e => e.Severity == DiagnosticSeverity.Error);
        }

        public static string[] GetCompileErrorMessages(this IEnumerable<SerializableDiagnostic> diagnostics)
        {
            return diagnostics?.Where(d => d.Severity == DiagnosticSeverity.Error)
                              .Select(d => d.Message)
                              .ToArray() ?? Array.Empty<string>();
        }
    }
}