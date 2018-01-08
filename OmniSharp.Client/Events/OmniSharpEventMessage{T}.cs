using System;

namespace OmniSharp.Client.Events
{
    public class OmniSharpEventMessage<T> : OmniSharpEventMessage
        where T : class, IOmniSharpEventBody
    {
        public OmniSharpEventMessage(
            string eventName,
            T body,
            int seq)
            : base(eventName, seq)
        {
            Body = body;
        }

        public T Body { get; }
    }
}
