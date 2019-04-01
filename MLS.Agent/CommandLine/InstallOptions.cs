using System.IO;
using WorkspaceServer.Packaging;

namespace MLS.Agent.CommandLine
{
    public class InstallOptions
    {
        public InstallOptions(DirectoryInfo addSource, string packageName, DirectoryInfo location = null)
        {
            AddSource = addSource;
            PackageName = packageName;
            Location = location ?? Package.DefaultPackagesDirectory;
        }

        public DirectoryInfo AddSource { get; }

        public string PackageName { get; }
        public DirectoryInfo Location { get; }
    }
}