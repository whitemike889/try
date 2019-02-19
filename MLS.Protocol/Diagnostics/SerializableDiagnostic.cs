using MLS.Protocol.Execution;
using Newtonsoft.Json;

namespace MLS.Protocol.Diagnostics
{
    public class SerializableDiagnostic
    {
        [JsonConstructor]
        public SerializableDiagnostic(
            int start,
            int end,
            string message,
            DiagnosticSeverity severity,
            string id,
            BufferId bufferId = null,
            string location = null)
        {
            Start = start;
            End = end;
            Message = message;
            Severity = severity;
            Id = id;
            Location = location;
        }

        public int Start { get; }

        public int End { get; }

        public string Message { get; }

        public DiagnosticSeverity Severity { get; }

        public string Id { get; }

        public string Location { get; }

        public BufferId BufferId { get; }
    }
}