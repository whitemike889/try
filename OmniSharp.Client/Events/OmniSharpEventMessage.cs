using System;

namespace OmniSharp.Client.Events
{
    public abstract class OmniSharpEventMessage : OmniSharpMessage
    {
        protected OmniSharpEventMessage(string @event, int seq) : base("event", seq)
        {
            if (string.IsNullOrWhiteSpace(@event))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(@event));
            }

            Event = @event;
        }

        public string Event { get; }
    }
}
