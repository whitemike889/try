using System;
using System.Collections.Generic;

namespace OmniSharp.Client
{
    public class InstrumentationRegionMap
    {
        public InstrumentationRegionMap(string fileToInstrument, IEnumerable<Microsoft.CodeAnalysis.Text.TextSpan> instrumentationRegions)
        {
            FileToInstrument = fileToInstrument;
            InstrumentationRegions = instrumentationRegions ?? Array.Empty<Microsoft.CodeAnalysis.Text.TextSpan> ();
        }

        public string FileToInstrument { get; }

        public IEnumerable<Microsoft.CodeAnalysis.Text.TextSpan> InstrumentationRegions { get; }
    }
}
