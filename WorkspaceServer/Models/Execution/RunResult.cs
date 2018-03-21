using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pocket;

namespace WorkspaceServer.Models.Execution
{
    [JsonConverter(typeof(RunResultJsonConverter))]
    public class RunResult : IDisposable
    {
        private readonly Dictionary<Type, object> features = new Dictionary<Type, object>();

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public RunResult(
            bool succeeded,
            IReadOnlyCollection<string> output = null,
            string exception = null,
            IReadOnlyCollection<SerializableDiagnostic> diagnostics = null)
        {
            Output = output ?? Array.Empty<string>();
            Succeeded = succeeded;
            Exception = exception;
            AddFeature(diagnostics ??
                       Array.Empty<SerializableDiagnostic>());
        }

        public void AddFeature<T>(T feature)
        {
            if (feature is IDisposable disposable)
            {
                _disposables.Add(disposable);
            }

            features.Add(typeof(T), feature);
        }

        public IReadOnlyCollection<SerializableDiagnostic> Diagnostics =>
            this.GetFeature<IReadOnlyCollection<SerializableDiagnostic>>();

        public bool Succeeded { get; }

        public IReadOnlyCollection<string> Output { get; }

        public string Exception { get; }

        public IReadOnlyDictionary<Type, object> Features => features;

        public bool HasFeature<T>() => features.ContainsKey(typeof(T));

        public override string ToString() =>
            $@"{nameof(Succeeded)}: {Succeeded}
{nameof(Output)}: {string.Join("\n", Output)}
{nameof(Exception)}: {Exception}";

        public void Dispose() => _disposables.Dispose();

        private class RunResultJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value is RunResult runResult)
                {
                    var o = new JObject
                    {
                        new JProperty("diagnostics", JArray.FromObject(runResult.Diagnostics)),
                        new JProperty("succeeded", new JValue(runResult.Succeeded)),
                        new JProperty("output", JArray.FromObject(runResult.Output)),
                        new JProperty("exception", runResult.Exception)
                    };

                    foreach (var feature in runResult.features.Values.OfType<IAddRunResultProperties>())
                    {
                        feature.Augment(runResult, AddProperty);
                    }

                    o.WriteTo(writer);

                    void AddProperty(string name, object value1)
                    {
                        var jToken = JToken.FromObject(value1);
                        o.Add(new JProperty(name, jToken));
                    }
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
