using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WorkspaceServer.Models.Execution
{
    public class RunResult
    {
        private readonly Dictionary<Type, object> features = new Dictionary<Type, object>();

        public RunResult(
            bool succeeded,
            IReadOnlyCollection<string> output = null,
            string exception = null,
            IReadOnlyCollection<SerializableDiagnostic> diagnostics = null)
        {
            Output = output ?? Array.Empty<string>();
            Succeeded = succeeded;
            Exception = exception;

            if (diagnostics != null)
            {
                AddFeature(diagnostics);
            }
        }

        private void AddFeature<T>(T feature) => features.Add(typeof(T), feature);

        public IReadOnlyCollection<SerializableDiagnostic> Diagnostics =>
            this.GetFeature<IReadOnlyCollection<SerializableDiagnostic>>() ??
            Array.Empty<SerializableDiagnostic>();

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
    }
}
