using System;
using Newtonsoft.Json.Linq;

namespace OmniSharp.Client.Events
{
    public class OmniSharpUnknownEventMessage : OmniSharpEventMessage
    {
        public OmniSharpUnknownEventMessage(
            JToken body, 
            string @event,
            int seq) : base(@event, seq)
        {
            Body = body;
        }

        public JToken Body { get; }
    }
}
