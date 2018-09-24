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

        public void Apply(RunResult runResult)
        {
            var diagnostics =
                this.OrderBy(d => d.Start)
                    .ThenBy(d => d.End);

            runResult.AddProperty("diagnostics", diagnostics);
        }
    }
}
