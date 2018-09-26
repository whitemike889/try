
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pocket;

namespace MLS.Protocol.Execution
{
    [JsonConverter(typeof(CompileResultJsonConverter))]
    public class CompileResult : FeatureContainer
    {
        private readonly List<string> _output = new List<string>();

        public CompileResult(
            bool succeeded,
            string base64assembly,
            IReadOnlyCollection<SerializableDiagnostic> diagnostics = null)
        {
            Succeeded = succeeded;
            Base64Assembly = base64assembly;
            AddFeature(new Diagnostics(diagnostics?.ToList() ??
                                       Array.Empty<SerializableDiagnostic>().ToList()));
        }

        public bool Succeeded { get; }

        public string Base64Assembly { get; }

        private class CompileResultJsonConverter : FeatureContainerConverter<CompileResult>
        {
            protected override void AddProperties(CompileResult result, JObject o)
            {
                o.Add(new JProperty("succeeded", result.Succeeded));
                o.Add(new JProperty("base64assembly", result.Base64Assembly));
            }
        }
    }
}
