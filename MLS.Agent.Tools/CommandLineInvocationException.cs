using System;

namespace MLS.Agent.Tools
{
    public class CommandLineInvocationException : Exception
    {
        private CommandLineResult result;

        public CommandLineInvocationException(CommandLineResult result) : base(
            $"{result.ExitCode}: {result?.Exception?.Message ?? string.Join("\n", result.Error)}", result.Exception)
        {
            this.result = result;
        }
    }
}
