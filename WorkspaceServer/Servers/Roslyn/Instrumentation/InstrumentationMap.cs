using System;
using System.Collections.Generic;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation
{
    public class InstrumentationMap
    {
        public InstrumentationMap(string fileToInstrument, IEnumerable<Microsoft.CodeAnalysis.Text.TextSpan> instrumentationRegions)
        {
            FileToInstrument = fileToInstrument;
            InstrumentationRegions = instrumentationRegions ?? Array.Empty<Microsoft.CodeAnalysis.Text.TextSpan>();
        }

        public string FileToInstrument { get; }

        public IEnumerable<Microsoft.CodeAnalysis.Text.TextSpan> InstrumentationRegions { get; }
    }
}
