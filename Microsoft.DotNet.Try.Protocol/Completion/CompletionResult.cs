using System;
using System.Collections.Generic;
using Microsoft.DotNet.Try.Protocol.Diagnostics;
using Newtonsoft.Json;

namespace Microsoft.DotNet.Try.Protocol.Completion
{

    public class CompletionResult
    {
        public CompletionItem[] Items { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RequestId { get; }

        public IEnumerable<SerializableDiagnostic> Diagnostics { get; }

        public CompletionResult(CompletionItem[] items = null, IEnumerable<SerializableDiagnostic> diagnostics = null, string requestId = null)
        {
            Items = items ?? Array.Empty<CompletionItem>();
            Diagnostics = diagnostics;
            RequestId = requestId;
        }
    }
}
