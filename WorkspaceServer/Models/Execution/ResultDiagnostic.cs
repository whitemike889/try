using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using OmniSharp.Client;

namespace WorkspaceServer.Models.Execution
{
    public class ResultDiagnostic
    {
        [JsonConstructor]
        public ResultDiagnostic(int start, int end, string message, DiagnosticSeverity severity, string id)
        {
            this.Start = start;
            this.End = end;
            this.Message = message;
            this.Severity = severity;
            this.Id = id; ;
        }

        public ResultDiagnostic(Microsoft.CodeAnalysis.Diagnostic d)
            : this(d.Location.SourceSpan.Start,
                    d.Location.SourceSpan.End,
                    d.GetMessage(),
                    d.Severity,
                    d.Descriptor.Id)
        {
        }

        public ResultDiagnostic(OmniSharp.Client.Diagnostic d)
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
