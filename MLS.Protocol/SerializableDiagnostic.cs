
using Newtonsoft.Json;

namespace MLS.Protocol
{
    public enum DiagnosticSeverity
    {
        /// <summary>
        /// Something that is an issue, as determined by some authority,
        /// but is not surfaced through normal means.
        /// There may be different mechanisms that act on these issues.
        /// </summary>
        Hidden,
        /// <summary>
        /// Information that does not indicate a problem (i.e. not prescriptive).
        /// </summary>
        Info,
        /// <summary>Something suspicious but allowed.</summary>
        Warning,
        /// <summary>
        /// Something not allowed by the rules of the language or other authority.
        /// </summary>
        Error,
    }

    public class SerializableDiagnostic
    {
        [JsonConstructor]
        public SerializableDiagnostic(int start, int end, string message, DiagnosticSeverity severity, string id)
        {
            Start = start;
            End = end;
            Message = message;
            Severity = severity;
            Id = id;
        }

        public int Start { get; }
        public int End { get; }
        public string Message { get; }
        public DiagnosticSeverity Severity { get; }
        public string Id { get; }
    }
}
