namespace MLS.WasmCodeRunner
{
    public class CompileResult
    {
        public bool succeeded { get; set; }
        public string base64assembly { get; set; }
        public SerializableDiagnostic[] diagnostics { get; set; }
    }
}
