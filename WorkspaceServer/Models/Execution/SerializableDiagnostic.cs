using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using OmniSharp.Client;

namespace WorkspaceServer.Models.Execution
{
    public class SerializableDiagnostic
    {
        [JsonConstructor]
        public SerializableDiagnostic(int start, int end, string message, DiagnosticSeverity severity, string id)
        {
            this.Start = start;
            this.End = end;
            this.Message = message;
            this.Severity = severity;
            this.Id = id; ;
        }

        public SerializableDiagnostic(Microsoft.CodeAnalysis.Diagnostic d)
            : this(d.Location?.SourceSpan.Start ?? throw new ArgumentException(nameof(d.Location)),
                    d.Location.SourceSpan.End,
                    d.GetMessage(),
                    d.Severity,
                    d.Descriptor.Id)
        {
        }

        public SerializableDiagnostic(Diagnostic d)
            : this(d.Location.SourceSpan.Start,
                    d.Location.SourceSpan.End,
                    d.Message,
                    d.Severity,
                    d.Id)
        {
        }

        public int Start { get; }
        public int End { get; }
        public string Message { get; }
        public DiagnosticSeverity Severity { get; }
        public string Id { get; }
    }
}
