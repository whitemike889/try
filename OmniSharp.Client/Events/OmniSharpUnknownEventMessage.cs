using System;
using Newtonsoft.Json.Linq;

namespace OmniSharp.Client.Events
{
    public class OmniSharpUnknownEventMessage : OmniSharpEventMessage
    {
        public OmniSharpUnknownEventMessage(
            JObject body, 
            string @event,
            int seq) : base(@event, seq)
        {
            Body = body;
        }

        public JObject Body { get; }
    }
}
