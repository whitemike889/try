using System;

namespace OmniSharp.Client.Commands
{
    public class OmniSharpCommandMessage
    {
        public OmniSharpCommandMessage(string command, int seq)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(command));
            }

            Command = command;
            Seq = seq;
        }

        public string Command { get; }

        public int Seq { get; }
    }
}