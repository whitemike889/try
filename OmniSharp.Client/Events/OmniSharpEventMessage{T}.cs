using System;

namespace OmniSharp.Client.Events
{
    public class OmniSharpEventMessage<T> : OmniSharpEventMessage
        where T : class, IOmniSharpEventBody
    {
        public OmniSharpEventMessage(T body, int seq) : base(body.Name, seq)
        {
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public T Body { get; }
    }
}
