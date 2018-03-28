using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;

namespace WorkspaceServer
{
    public class WorkspaceBuilder
    {
        private Workspace _workspace;

        private readonly List<Func<Workspace, Budget, Task>> _afterCreateActions = new List<Func<Workspace, Budget, Task>>();

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

        public bool RequiresPublish { get; set; }

        public void CreateUsingDotnet(string template) =>
            WorkspaceInitializer = new DotnetWorkspaceInitializer(
                template,
                WorkspaceName,
                AfterCreate);

        public void AddPackageReference(string packageId, string version = null)
        {
            _afterCreateActions.Add(async (workspace, budget) =>
            {
                var dotnet = new Dotnet(workspace.Directory);
                await dotnet.AddPackageReference(packageId, version);
            });
        }

        public async Task<Workspace> GetWorkspace(Budget budget = null)
        {
            if (_workspace == null)
            {
                await PrepareWorkspace(budget);
            }

            return _workspace;
        }

        private async Task PrepareWorkspace(Budget budget = null)
        {
            budget = budget ?? new Budget();

            _workspace = new Workspace(
                WorkspaceName, 
                WorkspaceInitializer);

            await _workspace.EnsureCreated(budget);

            await _workspace.EnsureBuilt(budget);

            if (RequiresPublish)
            {
                await _workspace.EnsurePublished(budget);
            }
        }

        private async Task AfterCreate(DirectoryInfo directoryInfo, Budget budget)
        {
            foreach (var action in _afterCreateActions)
            {
                await action(_workspace, budget);
            }
        }
    }
}
