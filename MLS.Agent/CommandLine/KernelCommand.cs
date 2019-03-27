using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using MLS.Jupyter;

namespace MLS.Agent.CommandLine
{
    public static class KernelCommand
    {
        public static async Task<int> Do(KernelOptions options, IConsole console)
        {
            var connectionInfo = ConnectionInformation.Load(options.ConnectionFile);
            var hb = new HearthBeatHandler(connectionInfo);
            hb.Start();

            throw new NotImplementedException();
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