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
        private readonly WorkspaceRegistry _registry;

        private Workspace _workspace;

        private readonly List<Func<Workspace, Budget, Task>> _afterCreateActions = new List<Func<Workspace, Budget, Task>>();

        public WorkspaceBuilder(WorkspaceRegistry registry, string workspaceName)
        {
            if (string.IsNullOrWhiteSpace(workspaceName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(workspaceName));
            }

            _registry = registry ?? throw new ArgumentNullException(nameof(registry));

            WorkspaceName = workspaceName;
        }

        public string WorkspaceName { get; }

        internal IWorkspaceInitializer WorkspaceInitializer { get; private set; }

        public bool RequiresPublish { get; set; }

        public void CreateCopyOf(string originalWorkspaceName) =>
            WorkspaceInitializer = new WorkspaceCopyInitializer(
                _registry,
                originalWorkspaceName);

        public void CreateUsingDotnet(string template) =>
            WorkspaceInitializer = new WorkspaceInitializer(
                template,
                WorkspaceName,
                AfterCreate);

        public void AddPackageReference(string packageId, string version = null)
        {
            _afterCreateActions.Add(async (workspace, budget) =>
            {
                var dotnet = new Dotnet(workspace.Directory);
                await dotnet.AddPackage(packageId, version);
            });
        }

        public async Task<Workspace> GetWorkspace(Budget budget = null)
        {
            if (_workspace == null)
            {
                await PrepareWorkspace(budget);
            }

            budget?.RecordEntry();
            return _workspace;
        }

        public WorkspaceInfo GetWorkpaceInfo()
        {
            WorkspaceInfo info = null;
            if (_workspace != null)
            {
                info = new WorkspaceInfo(
                    _workspace.Name,
                    _workspace.BuildTime,
                    _workspace.ConstructionTime,
                    _workspace.PublicationTime,
                    _workspace.CreationTime,
                    _workspace.ReadyTime
                );
            }

            return info;
        }

        private async Task PrepareWorkspace(Budget budget = null)
        {
            budget = budget ?? new Budget();

            _workspace = new Workspace(
                WorkspaceName,
                WorkspaceInitializer,
                requiresPublish: RequiresPublish);

            await _workspace.EnsureReady(budget);

            budget.RecordEntry();
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
