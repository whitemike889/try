using System.IO;

namespace MLS.Agent.CommandLine
{
    public class VerifyOptions
    {
        public VerifyOptions(DirectoryInfo rootDirectory, bool compile)
        {
            Compile = compile;
            RootDirectory = rootDirectory;
        }

        public bool Compile { get; }

        public DirectoryInfo RootDirectory { get; }
    }
}