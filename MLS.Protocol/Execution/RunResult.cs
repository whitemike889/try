using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pocket;

namespace MLS.Protocol.Execution
{
    [JsonConverter(typeof(RunResultJsonConverter))]
    public class RunResult : IDisposable
    {
        private readonly Dictionary<Type, object> _features = new Dictionary<Type, object>();

        private readonly List<string> _output = new List<string>(); 

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public RunResult(
            bool succeeded,
            IReadOnlyCollection<string> output = null,
            string exception = null,
            IReadOnlyCollection<SerializableDiagnostic> diagnostics = null)
        {
            if (output != null)
            {
                _output.AddRange(output);
            }
            Succeeded = succeeded;
            Exception = exception;
            AddFeature(new Diagnostics(diagnostics?.ToList() ??
                                       Array.Empty<SerializableDiagnostic>().ToList()));
        }

        public void AddFeature<T>(T feature)
            where T : IRunResultFeature
        {
            if (feature is IDisposable disposable)
            {
                _disposables.Add(disposable);
            }

            _features.Add(typeof(T), feature);
        }

        public bool Succeeded { get; }

        public IReadOnlyCollection<string> Output => _output;

        public string Exception { get; }

        public IReadOnlyDictionary<Type, object> Features => _features;

        public override string ToString() =>
            $@"{nameof(Succeeded)}: {Succeeded}
{nameof(Output)}: {string.Join("\n", Output)}
{nameof(Exception)}: {Exception}";

        public void Dispose() => _disposables.Dispose();

        private List<(string, object)> _featureProperties;

        private List<(string Name, object Value)> FeatureProperties => _featureProperties ?? (_featureProperties = new List<(string, object )>());

        public void AddProperty(string name, object value) => FeatureProperties.Add((name, value));

        private class RunResultJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value is RunResult result)
                {
                    var o = new JObject();

                    foreach (var feature in result.Features.Values.OfType<IRunResultFeature>())
                    {
                        feature.Apply(result);
                    }

                    o.Add(new JProperty("succeeded", result.Succeeded));
                    o.Add(new JProperty("output", result.Output));
                    o.Add(new JProperty("exception", result.Exception));

                    foreach (var property in result.FeatureProperties.OrderBy(p => p.Name))
                    {
                        var jToken = JToken.FromObject(property.Value, serializer);
                        o.Add(new JProperty(property.Name, jToken));
                    }

                    o.WriteTo(writer);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
                throw new NotImplementedException();

            public override bool CanRead { get; } = false;

            public override bool CanWrite { get; } = true;

            public override bool CanConvert(Type objectType) => objectType == typeof(RunResult);
        }
    }
}
