using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using MLS.Jupyter;
using Pocket;

namespace MLS.Agent.CommandLine
{
    public static class KernelCommand
    {
        private static Logger Log { get; } = new Logger(category:nameof(KernelCommand));
        public static  Task<int> Do(KernelOptions options, IConsole console)
        {
            var connectionInfo = ConnectionInformation.Load(options.ConnectionFile);
            Log.Info($"{nameof(connectionInfo)}", connectionInfo);
            var hb = new HearthBeatHandler(connectionInfo);
            hb.Start();
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