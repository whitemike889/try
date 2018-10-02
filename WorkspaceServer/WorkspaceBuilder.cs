using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Workspaces;

namespace WorkspaceServer
{
    public class WorkspaceBuilder
    {
        private readonly WorkspaceRegistry _registry;

        private WorkspaceBuild workspaceBuild;

        private readonly List<Func<WorkspaceBuild, Budget, Task>> _afterCreateActions = new List<Func<WorkspaceBuild, Budget, Task>>();

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

        public void AfterCreate(Func<WorkspaceBuild, Budget, Task> action)
        {
            _afterCreateActions.Add(action);
        }

        public void CreateCopyOf(string originalWorkspaceName) =>
            WorkspaceInitializer = new WorkspaceCopyInitializer(
                _registry,
                originalWorkspaceName);

        public void CreateUsingDotnet(string template, string projectName = null) =>
            WorkspaceInitializer = new WorkspaceInitializer(
                template,
                projectName ?? WorkspaceName,
                AfterCreate);

        public void AddPackageReference(string packageId, string version = null)
        {
            _afterCreateActions.Add(async (workspace, budget) =>
            {
                var dotnet = new Dotnet(workspace.Directory);
                await dotnet.AddPackage(packageId, version);
            });
        }

        public void DeleteFile(string relativePath)
        {
            _afterCreateActions.Add(async (workspace, budget) =>
            {
                var filePath = Path.Combine(workspace.Directory.FullName, relativePath);
                File.Delete(filePath);
            });
        }

        public async Task<WorkspaceBuild> GetWorkspaceBuild(Budget budget = null)
        {
            if (workspaceBuild == null)
            {
                await PrepareWorkspace(budget);
            }

            budget?.RecordEntry();
            return workspaceBuild;
        }

        public WorkspaceInfo GetWorkpaceInfo()
        {
            WorkspaceInfo info = null;
            if (workspaceBuild != null)
            {
                info = new WorkspaceInfo(
                    workspaceBuild.Name,
                    workspaceBuild.BuildTime,
                    workspaceBuild.ConstructionTime,
                    workspaceBuild.PublicationTime,
                    workspaceBuild.CreationTime,
                    workspaceBuild.ReadyTime
                );
            }

            return info;
        }

        private async Task PrepareWorkspace(Budget budget = null)
        {
            budget = budget ?? new Budget();

            workspaceBuild = new WorkspaceBuild(
                WorkspaceName,
                WorkspaceInitializer,
                requiresPublish: RequiresPublish);

            await workspaceBuild.EnsureReady(budget);

            budget.RecordEntry();
        }

        private async Task AfterCreate(DirectoryInfo directoryInfo, Budget budget)
        {
            foreach (var action in _afterCreateActions)
            {
                await action(workspaceBuild, budget);
            }
        }
    }
}
