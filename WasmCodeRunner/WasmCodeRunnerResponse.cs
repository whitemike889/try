using Newtonsoft.Json;
using Recipes;

namespace MLS.WasmCodeRunner
{
    public class WasmCodeRunnerResponse
    {
        public WasmCodeRunnerResponse(bool succeeded, string exception, string[] output, SerializableDiagnostic[] diagnostics, string runnerException)
        {
            Exception = exception;
            Output = output;
            Succeeded = succeeded;
            Diagnostics = diagnostics;
            RunnerException = runnerException;
        }

        [JsonProperty("exception")]
        public string Exception { get; }
        [JsonProperty("output")]
        public string[] Output { get; }
        [JsonProperty("succeeded")]
        public bool Succeeded { get; }
        [JsonProperty("diagnostics")]
        public SerializableDiagnostic[] Diagnostics { get; }
        [JsonProperty("runnerException")]
        public string RunnerException { get; }
        [JsonProperty("codeRunnerVersion")]
        public string CodeRunnerVersion => VersionSensor.Version().AssemblyInformationalVersion;

    }
}