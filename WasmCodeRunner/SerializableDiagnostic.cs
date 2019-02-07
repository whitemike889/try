using Newtonsoft.Json;

namespace MLS.WasmCodeRunner
{
    public class SerializableDiagnostic
    {
        [JsonConstructor]
        public SerializableDiagnostic(int start, int end, string message, int severity, string id)
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
        public int Severity { get; }
        public string Id { get; }
    }
}
