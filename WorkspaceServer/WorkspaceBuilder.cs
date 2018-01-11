using System;
using System.Threading.Tasks;
using MLS.Agent.Tools;

namespace WorkspaceServer
{
    public class WorkspaceBuilder
    {
        private Workspace _workspace;

        public WorkspaceBuilder(string workspaceName)
        {
            if (string.IsNullOrWhiteSpace(workspaceName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(workspaceName));
            }

            WorkspaceName = workspaceName;
        }

        public string WorkspaceName { get; }

        internal IWorkspaceInitializer WorkspaceInitializer { get; private set; }

        public void CreateUsingDotnet(string template) =>
            WorkspaceInitializer = new DotnetWorkspaceInitializer(template, WorkspaceName);

        public async Task<Workspace> GetWorkspace()
        {
            if (_workspace == null)
            {
                _workspace = new Workspace(WorkspaceName);
                await _workspace.EnsureCreated();
                _workspace.EnsureBuilt();
            }

            return _workspace;
        }
    }
}
