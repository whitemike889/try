using System;

namespace OmniSharp.Client.Commands
{
    public class OmniSharpCommandMessage<T> : OmniSharpCommandMessage
        where T : class, IOmniSharpCommandArguments
    {
        public OmniSharpCommandMessage(T arguments, int seq) : 
            base(arguments?.Command, seq)
        {
            Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }

        public T Arguments { get; }
    }
}
