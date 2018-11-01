using System;
using System.Collections.Generic;

namespace MLS.Protocol.Completion
{
    public class CompletionResult
    {
        public CompletionItem[] Items { get; }

        public string CorrelationId { get; }

        public IEnumerable<SerializableDiagnostic> Diagnostics { get; }

        public CompletionResult(CompletionItem[] items = null, IEnumerable<SerializableDiagnostic> diagnostics = null, string correlationId = null)
        {
            Items = items ?? Array.Empty<CompletionItem>();
            Diagnostics = diagnostics;
            CorrelationId = correlationId;
        }
    }
}
