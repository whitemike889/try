using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using MLS.Jupyter;
using Pocket;

namespace MLS.Agent.CommandLine
{
    public static class KernelCommand
    {
        private static Logger Log { get; } = new Logger(category: nameof(KernelCommand));
        public static Task<int> Do(
            KernelOptions options, 
            IConsole console,
            CommandLineParser.StartServer startServer = null,
            InvocationContext context = null)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            var connectionInfo = ConnectionInformation.Load(options.ConnectionFile);
            Log.Info($"{nameof(connectionInfo)}", connectionInfo);
            var shell = new Shell(connectionInfo);
            shell.Start();

            var hb = new HearthBeatHandler(connectionInfo);
            hb.Start();

            startServer?.Invoke(new StartupOptions(), context);

            return Task.FromResult(0);
        }
    }

    public class KernelOptions
    {
        public KernelOptions(FileInfo connectionFile)
        {
            ConnectionFile = connectionFile;
        }
        public FileInfo ConnectionFile { get; }
    }
}