using System.IO;

namespace MLS.Agent.CommandLine
{
    public class VerifyOptions
    {
        public VerifyOptions(DirectoryInfo rootDirectory)
        {
            RootDirectory = rootDirectory;
        }

        public DirectoryInfo RootDirectory { get; }
    }
}