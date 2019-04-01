using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace MLS.Agent.CommandLine
{
    public static class JupyterCommand
    {
        public static Task<int> Do(
            JupyterOptions options,
            IConsole console,
            CommandLineParser.StartServer startServer = null,
            InvocationContext context = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            startServer?.Invoke(new StartupOptions(), context);

            return Task.FromResult(0);
        }
    }
}