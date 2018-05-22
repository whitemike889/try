using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;
using Pocket;
using Recipes;
using WorkspaceServer.Servers.Dotnet;
using static Pocket.Logger<WorkspaceServer.WorkspaceServerRegistry>;

namespace WorkspaceServer
{
    public class WorkspaceServerRegistry : IDisposable
    {
        private readonly Dictionary<string, WorkspaceBuilder> _workspaceBuilders = new Dictionary<string, WorkspaceBuilder>();

        private readonly ConcurrentDictionary<string, IWorkspaceServer> _workspaceServers = new ConcurrentDictionary<string, IWorkspaceServer>();
       
        public void AddWorkspace(string name, Action<WorkspaceBuilder> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            var options = new WorkspaceBuilder(name);
            configure(options);
            _workspaceBuilders.Add(name, options);
        }

        public Task<Workspace> GetWorkspace(string workspaceName, TimeBudget budget = null) =>
            _workspaceBuilders.GetOrAdd(
                workspaceName,
                name =>
                {
                    var directory = new DirectoryInfo(
                        Path.Combine(
                            Workspace.DefaultWorkspacesDirectory.FullName, workspaceName));

                    if (directory.Exists)
                    {
                        return new WorkspaceBuilder(name);
                    }

                    throw new ArgumentException($"Workspace named \"{name}\" not found.");
                }).GetWorkspace(budget);

        public async Task<IWorkspaceServer> GetWorkspaceServer(string name, TimeBudget budget = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            IWorkspaceServer server;
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                budget?.RecordEntry();
                var workspace = await GetWorkspace(name, budget);
                server = _workspaceServers.GetOrAdd(name, _ => new DotnetWorkspaceServer(workspace));
                operation.Succeed();
            }

            return server;
        }

        public void Dispose()
        {
            foreach (var workspaceServer in _workspaceServers.Values.OfType<IDisposable>())
            {
                workspaceServer.Dispose();
            }
        }

        public async Task StartAllServers(Budget budget = null)
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                await Task.WhenAll(_workspaceBuilders.Keys.Select(async name =>
                {
                    var workspaceServer = await GetWorkspaceServer(name);
                    if (workspaceServer is DotnetWorkspaceServer dotnetWorkspaceServer)
                    {
                        await dotnetWorkspaceServer.EnsureInitializedAndNotDisposed(budget);
                    }
                }));

                operation.Succeed();
            }
        }

        public IEnumerable<WorkspaceInfo> GetRegisterWorkspaceInfos()
        {
            var workspaceInfos = _workspaceBuilders?.Values.Select(wb => wb.GetWorkpaceInfo()).Where(info => info != null).ToArray() ?? Array.Empty<WorkspaceInfo>();

            return workspaceInfos;
        }
    }
}
