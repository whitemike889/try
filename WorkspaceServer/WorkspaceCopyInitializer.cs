using System.IO;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Workspaces;

namespace WorkspaceServer
{
    public class WorkspaceCopyInitializer : IWorkspaceInitializer
    {
        private readonly WorkspaceRegistry registry;
        private readonly string workspaceName;

        public WorkspaceCopyInitializer(WorkspaceRegistry registry, string workspaceName)
        {
            this.registry = registry;
            this.workspaceName = workspaceName;
        }

        public async Task Initialize(DirectoryInfo directory, Budget budget = null)
        {
            var original = await registry.Get(workspaceName);
            await WorkspaceBuild.Copy(original, directory);
        }
    }
}