using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.DotNet.Try.Protocol
{
    public class ProjectDiagnostics : ReadOnlyCollection<SerializableDiagnostic>, IRunResultFeature
    {
        public ProjectDiagnostics(IEnumerable<SerializableDiagnostic> diagnostics) : base(diagnostics.ToArray())
        {
        }

        public string Name => nameof(ProjectDiagnostics);

        public void Apply(FeatureContainer result)
        {
            result.AddProperty("projectDiagnostics", this.Sort());
        }
    }

    public class CompilationEntryPoint : IRunResultFeature
    {
        public bool IsAsync { get; }
        public string FullyQualifiedName { get; }
        public SignatureParameter[] Parameters { get; }

        public CompilationEntryPoint(string fullyQualifiedName, IEnumerable<SignatureParameter> parameters, bool isAsync)
        {
            if (string.IsNullOrWhiteSpace(fullyQualifiedName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(fullyQualifiedName));
            }

            IsAsync = isAsync;
            FullyQualifiedName = fullyQualifiedName;
            Parameters = parameters?.ToArray() ?? Array.Empty<SignatureParameter>();
        }
        public string Name => nameof(CompilationEntryPoint);

        public void Apply(FeatureContainer result)
        {
            result.AddProperty("entryPoint", new
            {
                fullyQualifiedName = FullyQualifiedName,
                parameters = Parameters,
                isAsync = IsAsync
            });
        }
    }

    public class SignatureParameter
    {
        [JsonProperty("ordinal")]
        public int Ordinal { get; }
        [JsonProperty("name")]
        public string Name { get; }
        [JsonProperty("type")]
        public string Type { get; }

        public SignatureParameter(int ordinal, string name, string type)
        {
            Ordinal = ordinal;
            Name = name;
            Type = type;
        }
    }
}