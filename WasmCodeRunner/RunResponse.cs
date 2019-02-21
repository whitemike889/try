using Recipes;

namespace MLS.WasmCodeRunner
{
    public class RunResponse
    {

        public RunResponse(bool succeeded, string exception, string[] output, SerializableDiagnostic[] diagnostics, string runnerException)
        {
            this.exception = exception;
            this.output = output;
            this.succeeded = succeeded;
            this.diagnostics = diagnostics;
            this.runnerException = runnerException;
        }

        public string exception { get;}
        public string[] output { get; }
        public bool succeeded { get; }
        public SerializableDiagnostic[] diagnostics { get; }
        public string runnerException { get; }
        public string codeRunnerVersion => VersionSensor.Version().AssemblyInformationalVersion;

    }
}
