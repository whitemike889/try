using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pocket;

namespace MLS.Protocol.Execution
{
    public abstract class FeatureContainer : IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly Dictionary<string, object> _features =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private List<(string, object)> _featureProperties;


        public IReadOnlyDictionary<string, object> Features => _features;

        public void Dispose() => _disposables.Dispose();

        public void AddFeature(IRunResultFeature feature)
        {
            if (feature is IDisposable disposable)
            {
                _disposables.Add(disposable);
            }

            _features.Add(feature.Name, feature);
        }

        public List<(string Name, object Value)> FeatureProperties => _featureProperties ?? (_featureProperties = new List<(string, object)>());

        public void AddProperty(string name, object value) => FeatureProperties.Add((name, value));
    }

    public abstract class FeatureContainerConverter<T> : JsonConverter where T : FeatureContainer
    {
        protected abstract void AddProperties(T result, JObject o);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is T result)
            {
                var o = new JObject();

                AddProperties(result, o);

                foreach (var feature in result.Features.Values.OfType<IRunResultFeature>())
                {
                    feature.Apply(result);
                }

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
