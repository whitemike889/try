using System.IO;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;

namespace WorkspaceServer.Packaging
{
    public class PackageToolInitializer : IPackageInitializer
    {
        // QUESTION-JOSEQU: (PackageToolInitializer) is this used?
        private readonly string _toolName;

        public PackageToolInitializer(string toolName)
        {
            _toolName = toolName;
        }

        public Task Initialize(DirectoryInfo directory, Budget budget = null)
        {
            return CommandLine.Execute(_toolName, "extract-package", budget: budget);
        }
    }
}
