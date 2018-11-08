using System;
using Microsoft.CodeAnalysis;
using MLS.Protocol;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;
using MLSDiagnosticSeverity = MLS.Protocol.DiagnosticSeverity;

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
        public static SerializableDiagnostic ToSerializableDiagnostic(this Diagnostic diagnostic, string message = null)
        {
            return new SerializableDiagnostic(diagnostic.Location?.SourceSpan.Start ?? throw new ArgumentException(nameof(diagnostic.Location)),
                diagnostic.Location.SourceSpan.End,
                message ?? diagnostic.GetMessage(),
                diagnostic.ConvertSeverity(),
                diagnostic.Descriptor.Id);
        }
    }
}
