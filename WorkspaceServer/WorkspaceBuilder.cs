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

        private readonly List<Func<Workspace, TimeBudget, Task>> _afterCreateActions = new List<Func<Workspace, TimeBudget, Task>>();

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

        public void AddPackageReference(string packageId, string version = null)
        {
            _afterCreateActions.Add((workspace, budget) =>
            {
                var dotnet = new Dotnet(workspace.Directory);

                var versionArg = string.IsNullOrWhiteSpace(version)
                                     ? ""
                                     : $"--version {version}";

                dotnet.Execute($"add package {versionArg} {packageId}", budget);

                return Task.CompletedTask;
            });
        }

        public async Task<Workspace> GetWorkspace(TimeBudget budget = null)
        {
            if (_workspace == null)
            {
                await BuildWorkspace(budget);
            }

            return _workspace;
        }

        private async Task BuildWorkspace(TimeBudget budget = null)
        {
            budget = budget ?? TimeBudget.Unlimited();

            _workspace = new Workspace(WorkspaceName);

            await _workspace.EnsureCreated(budget);

            foreach (var action in _afterCreateActions)
            {
                await action(_workspace, budget);
            }

            await _workspace.EnsureBuilt(budget);
        }
    }
}
