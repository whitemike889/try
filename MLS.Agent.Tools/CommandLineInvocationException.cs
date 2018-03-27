using System;

namespace MLS.Agent.Tools
{
    public class CommandLineInvocationException : Exception
    {
        public CommandLineInvocationException(CommandLineResult result) : base(
            $"{result.ExitCode}: {string.Join("\n", result.Error)}")
        {
        }
    }
}
