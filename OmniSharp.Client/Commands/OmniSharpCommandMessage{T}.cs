using System;

namespace OmniSharp.Client.Commands
{
    public class OmniSharpCommandMessage<T> where T: class, IOmniSharpCommandBody
    {
        public OmniSharpCommandMessage(T body, int seq)
        {
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Seq = seq;
        }

        public T Body { get; }

        public string Command => Body.Command;

        public int Seq { get; }
    }
}
