using OmniSharp.Mef;
using OmniSharp.Models;

namespace OmniSharp.Emit
{
    [OmniSharpEndpoint(EmitService.EndpointName, typeof(EmitRequest), typeof(EmitResponse))]
    public class EmitRequest : Request
    {
        public string Language { get; set; }
    }
}
