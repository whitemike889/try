using System;
using Microsoft.CodeAnalysis;
using MLS.Protocol.Diagnostics;
using MLS.Protocol.Execution;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;
using MLSDiagnosticSeverity = MLS.Protocol.Diagnostics.DiagnosticSeverity;

namespace MLS.Project.Extensions
{
    public static class DiagnosticExtensions
    {
        public static MLSDiagnosticSeverity ConvertSeverity(this Diagnostic diagnostic)
        {
            switch (diagnostic.Severity)
            {
                case DiagnosticSeverity.Hidden:
                    return MLSDiagnosticSeverity.Hidden;
                case DiagnosticSeverity.Info:
                    return MLSDiagnosticSeverity.Info;
                case DiagnosticSeverity.Warning:
                    return MLSDiagnosticSeverity.Warning;
                case DiagnosticSeverity.Error:
                    return MLSDiagnosticSeverity.Error;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static SerializableDiagnostic ToSerializableDiagnostic(
            this Diagnostic diagnostic,
            string message = null,
            BufferId bufferId = null)
        {
            var diagnosticMessage = diagnostic.GetMessage();

            var startPosition = diagnostic.Location.GetLineSpan().Span.Start;

            var location =
                diagnostic.Location != null
                    ? $"{diagnostic.Location.SourceTree?.FilePath}({startPosition.Line + 1},{startPosition.Character + 1}): {GetMessagePrefix()}"
                    : null;

            return new SerializableDiagnostic(diagnostic.Location?.SourceSpan.Start ?? throw new ArgumentException(nameof(diagnostic.Location)),
                                              diagnostic.Location.SourceSpan.End,
                                              message ?? diagnosticMessage,
                                              diagnostic.ConvertSeverity(),
                                              diagnostic.Descriptor.Id,
                                              bufferId,
                                              location);

            string GetMessagePrefix()
            {
                string prefix;
                switch (diagnostic.Severity)
                {
                    case DiagnosticSeverity.Hidden:
                        prefix = "hidden";
                        break;
                    case DiagnosticSeverity.Info:
                        prefix = "info";
                        break;
                    case DiagnosticSeverity.Warning:
                        prefix = "warning";
                        break;
                    case DiagnosticSeverity.Error:
                        prefix = "error";
                        break;
                    default:
                        return null;
                }

                return $"{prefix} {diagnostic.Id}";
            }
        }
    }
}
