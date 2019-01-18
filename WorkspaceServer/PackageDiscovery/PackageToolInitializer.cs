using System.IO;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;
using WorkspaceServer.Packaging;

namespace WorkspaceServer.PackageDiscovery
{
    public class PackageToolInitializer : IPackageInitializer
    {
        private readonly string _toolName;
        private readonly DirectoryInfo _workingDirectory;

        public PackageToolInitializer(string toolName, DirectoryInfo workingDirectory)
        {
            _toolName = toolName;
            _workingDirectory = workingDirectory;
        }

        public Task Initialize(DirectoryInfo directory, Budget budget = null)
        {
            return CommandLine.Execute(_toolName, "extract-package", budget: budget);
        }
    }
}
