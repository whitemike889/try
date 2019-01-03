using System.IO;

namespace MLS.Agent.Markdown
{
    public class Configuration
    {
        public DirectoryInfo RootDirectory { get; set; }

        public Configuration(DirectoryInfo rootDir)
        {
            RootDirectory = rootDir;
        }
    }
}
