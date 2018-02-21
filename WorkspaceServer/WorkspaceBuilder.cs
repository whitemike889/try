using System;
using System.Collections.Generic;
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

                var versionArg = string.IsNullOrWhiteSpace(version)
                                     ? ""
                                     : $"--version {version}";

                await dotnet.Execute($"add package {versionArg} {packageId}", budget);
            });
        }

        public async Task<Workspace> GetWorkspace(Budget budget = null)
        {
            if (_workspace == null)
            {
                await BuildWorkspace(budget);
            }

            return _workspace;
        }

        private async Task BuildWorkspace(Budget budget = null)
        {
            budget = budget ?? new Budget();

            _workspace = new Workspace(
                WorkspaceName, 
                WorkspaceInitializer);

            await _workspace.EnsureCreated(budget);

            await _workspace.EnsureBuilt(budget);
        }

        private async Task AfterCreate(Budget budget)
        {
            foreach (var action in _afterCreateActions)
            {
                await action(_workspace, budget);
            }
        }
    }
}
