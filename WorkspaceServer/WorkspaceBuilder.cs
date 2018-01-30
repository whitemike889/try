using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;

namespace WorkspaceServer
{
    public class WorkspaceBuilder
    {
        private Workspace _workspace;

        private readonly List<Func<Workspace, CancellationToken, Task>> _afterCreateActions = new List<Func<Workspace, CancellationToken, Task>>();

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
            _afterCreateActions.Add((workspace, cancellationToken) =>
            {
                var dotnet = new Dotnet(workspace.Directory);

                var versionArg = string.IsNullOrWhiteSpace(version)
                                     ? ""
                                     : $"--version {version}";

                dotnet.Execute($"add package {versionArg} {packageId}",
                               cancellationToken);

                return Task.CompletedTask;
            });
        }

        public async Task<Workspace> GetWorkspace(CancellationToken? cancellationToken = null)
        {
            if (_workspace == null)
            {
                await BuildWorkspace(cancellationToken);
            }

            return _workspace;
        }

        private async Task BuildWorkspace(CancellationToken? cancellationToken)
        {
            cancellationToken = cancellationToken ?? Clock.Current.CreateCancellationToken(TimeSpan.FromSeconds(55));

            _workspace = new Workspace(WorkspaceName);

            await _workspace.EnsureCreated(cancellationToken);

            foreach (var action in _afterCreateActions)
            {
                await action(_workspace, cancellationToken.Value);
            }

            _workspace.EnsureBuilt(cancellationToken);
        }
    }
}
