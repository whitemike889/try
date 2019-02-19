using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MLS.Protocol.Diagnostics;

namespace MLS.Protocol.Execution
{
    public class Diagnostics : ReadOnlyCollection<SerializableDiagnostic>, IRunResultFeature
    {
        public Diagnostics(IList<SerializableDiagnostic> list) : base(list)
        {
        }

        public string Name => nameof(Diagnostics);

        public void Apply(FeatureContainer result)
        {
            result.AddProperty("diagnostics", this.Sort());
        }
    }
}