using System;

namespace MLS.Agent.Tools
{
    public static class CommandLineResultExtensions
    {
        public static void ThrowOnFailure(this CommandLineResult result)
        {
            if (result.ExitCode != 0)
            {
                throw new CommandLineInvocationException(result);
            }
        }
    }
}
