using System.IO;

namespace MLS.Agent.CommandLine
{
    public class InstallOptions
    {
        public InstallOptions(DirectoryInfo addSource, string packageName)
        {
            AddSource = addSource;
            PackageName = packageName;
        }

        public DirectoryInfo AddSource { get; }

        public string PackageName { get; }
    }
}