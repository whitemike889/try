using System.IO;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Packaging;

namespace WorkspaceServer
{
    public class PackageCopyInitializer : IPackageInitializer
    {
        private readonly PackageRegistry registry;
        private readonly string workspaceName;

        public PackageCopyInitializer(PackageRegistry registry, string workspaceName)
        {
            this.registry = registry;
            this.workspaceName = workspaceName;
        }

        public async Task Initialize(DirectoryInfo directory, Budget budget = null)
        {
            var original = await registry.Get(workspaceName);
            await Package.Copy(original, directory);
        }
    }
}