using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
            var diagnostics =
                this.OrderBy(d => d.Start)
                    .ThenBy(d => d.End);

            result.AddProperty("diagnostics", diagnostics);
        }
    }
}
