using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.WorkspaceFeatures
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
