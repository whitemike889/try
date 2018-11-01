using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pocket;

namespace MLS.Protocol.Execution
{
    [JsonConverter(typeof(RunResultJsonConverter))]
    public class RunResult : FeatureContainer
    {

        private readonly List<string> _output = new List<string>(); 

        public RunResult(
            bool succeeded,
            IReadOnlyCollection<string> output = null,
            string exception = null,
            IEnumerable<SerializableDiagnostic> diagnostics = null, string correlationId = null)
        {
            if (output != null)
            {
                _output.AddRange(output);
            }

            CorrelationId = correlationId;
            Succeeded = succeeded;
            Exception = exception;
            AddFeature(new Diagnostics(diagnostics?.ToList() ??
                                       Array.Empty<SerializableDiagnostic>().ToList()));
        }

        public string CorrelationId { get; }
        public bool Succeeded { get; }

        public IReadOnlyCollection<string> Output => _output;

        public string Exception { get; }



        public override string ToString() =>
            $@"{nameof(Succeeded)}: {Succeeded}
{nameof(Output)}: {string.Join("\n", Output)}
{nameof(Exception)}: {Exception}";

        private class RunResultJsonConverter : FeatureContainerConverter<RunResult>
        {
            protected override void AddProperties(RunResult result, JObject o)
            {
                o.Add(new JProperty("correlationId", result.CorrelationId));
                o.Add(new JProperty("succeeded", result.Succeeded));
                o.Add(new JProperty("output", result.Output));
                o.Add(new JProperty("exception", result.Exception));
            }
        }
    }
}
