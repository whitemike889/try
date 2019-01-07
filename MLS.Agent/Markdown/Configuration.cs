using System.IO;

namespace MLS.Agent.Markdown
{
    public class Configuration
    {
        public DirectoryInfo RootDirectory { get; }

        public Configuration(DirectoryInfo rootDir)
        {
            RootDirectory = rootDir;
        }
    }
}
