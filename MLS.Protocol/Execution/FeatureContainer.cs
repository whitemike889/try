using System;
using System.Collections.Generic;
using System.Text;
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
}
