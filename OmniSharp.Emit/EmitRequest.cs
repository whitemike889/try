using OmniSharp.Mef;
using OmniSharp.Models;
using System.Collections.Generic;

namespace OmniSharp.Emit
{
    [OmniSharpEndpoint(EmitService.EndpointName, typeof(EmitRequest), typeof(EmitResponse))]
    public class EmitRequest : Request
    {
        public string Language { get; set; }

        public bool IncludeInstrumentation { get; set; }

        public IEnumerable<InstrumentationMap> InstrumentationRegions { get; set; }
    }
}
