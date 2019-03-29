using System.IO;

namespace MLS.Agent.CommandLine
{
    public class JupyterOptions
    {
        public JupyterOptions(FileInfo connectionFile)
        {
            ConnectionFile = connectionFile;
        }

        public FileInfo ConnectionFile { get; }
    }
}