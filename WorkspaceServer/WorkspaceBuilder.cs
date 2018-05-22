using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;
using Pocket;
using static Pocket.Logger<WorkspaceServer.WorkspaceBuilder>;

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
                    _workspace.InitialisedTime
                );
            }

            return info;
        }

        private async Task PrepareWorkspace(Budget budget = null)
        {
            budget = budget ?? new Budget();
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                _workspace = new Workspace(
                    WorkspaceName,
                    WorkspaceInitializer);

                await _workspace.EnsureCreated(budget);

                await _workspace.EnsureBuilt(budget);

                if (RequiresPublish)
                {
                    await _workspace.EnsurePublished(budget);
                }

                budget?.RecordEntry();
                operation.Succeed();
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
