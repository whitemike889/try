using System;

namespace MLS.Agent.Tools
{
    public class CommandLineInvocationException : Exception
    {
        public CommandLineInvocationException(CommandLineResult result, string message = null) : base(
            $"{message}{Environment.NewLine}Exit code {result.ExitCode}: {string.Join("\n", result.Error)}".Trim())
        {
        }
    }
}
