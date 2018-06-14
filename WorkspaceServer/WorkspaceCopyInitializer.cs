using System.IO;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;

namespace WorkspaceServer
{
    public class WorkspaceCopyInitializer : IWorkspaceInitializer
    {
        private readonly DotnetWorkspaceServerRegistry registry;
        private readonly string workspaceName;

        public WorkspaceCopyInitializer(DotnetWorkspaceServerRegistry registry, string workspaceName)
        {
            this.registry = registry;
            this.workspaceName = workspaceName;
        }

        public async Task Initialize(DirectoryInfo directory, Budget budget = null)
        {
            var original = await registry.GetWorkspace(workspaceName);
            Workspace.Copy(original, directory);
        }
    }
}