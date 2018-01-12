using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MLS.Agent.Tools;

namespace WorkspaceServer
{
    public class WorkspaceBuilder
    {
        private Workspace _workspace;

        private readonly List<Func<Workspace, Task>> _afterCreateActions = new List<Func<Workspace, Task>>();

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
            WorkspaceInitializer = new DotnetWorkspaceInitializer(
                template,
                WorkspaceName);

        public void AddPackageReference(string packageId)
        {
            _afterCreateActions.Add(workspace =>
            {
                var dotnet = new Dotnet(workspace.Directory);
                dotnet.Execute($"add package {packageId}");
                return Task.CompletedTask;
            });
        }

        public async Task<Workspace> GetWorkspace()
        {
            if (_workspace == null)
            {
                _workspace = new Workspace(WorkspaceName);
                await _workspace.EnsureCreated();

                foreach (var action in _afterCreateActions)
                {
                    await action(_workspace);
                }

                _workspace.EnsureBuilt();
            }

            return _workspace;
        }
    }
}
