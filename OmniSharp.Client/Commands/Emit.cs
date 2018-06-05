using System;
using System.Collections.Generic;

namespace OmniSharp.Client.Commands
{
    public class Emit : AbstractOmniSharpCommandArguments
    {
        public Emit(bool includeInstrumentation = false, IEnumerable<InstrumentationRegionMap> instrumentationRegions = null)
        {
            this.IncludeInstrumentation = includeInstrumentation;
            this.InstrumentationRegions = instrumentationRegions ?? Array.Empty<InstrumentationRegionMap>();
        }

        public override string Command => "/emit";

        public bool IncludeInstrumentation { get; }

        public IEnumerable<InstrumentationRegionMap> InstrumentationRegions { get; }
    }
}
