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
}