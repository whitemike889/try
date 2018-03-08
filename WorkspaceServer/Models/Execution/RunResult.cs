using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Pocket;

namespace WorkspaceServer.Models.Execution
{
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

        [JsonIgnore]
        public IReadOnlyDictionary<Type, object> Features => features;

        public bool HasFeature<T>() => features.ContainsKey(typeof(T));

        public override string ToString() =>
            $@"{nameof(Succeeded)}: {Succeeded}
{nameof(Output)}: {string.Join("\n", Output)}
{nameof(Exception)}: {Exception}";

        public void Dispose() => _disposables.Dispose();
    }
}
