using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using Pocket;
using WorkspaceServer.Servers.OmniSharp;
using static Pocket.Logger<WorkspaceServer.WorkspaceServerRegistry>;

namespace WorkspaceServer
{
    public class WorkspaceServerRegistry : IDisposable
    {
        private readonly Dictionary<string, WorkspaceBuilder> workspaceBuilders = new Dictionary<string, WorkspaceBuilder>();

        private readonly ConcurrentDictionary<string, IWorkspaceServer> workspaceServers = new ConcurrentDictionary<string, IWorkspaceServer>();

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
            workspaceBuilders.Add(name, options);
        }

        public Task<Workspace> GetWorkspace(string workspaceId) =>
            workspaceBuilders[workspaceId].GetWorkspace();

        public async Task<IWorkspaceServer> GetWorkspaceServer(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            var workspace = await GetWorkspace(name);

            return workspaceServers.GetOrAdd(name, _ =>
            {
                return new DotnetWorkspaceServer(workspace);
            });
        }

        public void Dispose()
        {
            foreach (var workspaceServer in workspaceServers.Values.OfType<IDisposable>())
            {
                workspaceServer.Dispose();
            }
        }

        public async Task StartAllServers()
        {
            using (var operation = Log.ConfirmOnExit())
            {
                await Task.WhenAll(workspaceBuilders.Keys.Select(async name =>
                {
                    var workspaceServer = await GetWorkspaceServer(name);
                    if (workspaceServer is DotnetWorkspaceServer dotnetWorkspaceServer)
                    {
                        await dotnetWorkspaceServer.EnsureInitializedAndNotDisposed(timeout: TimeSpan.FromSeconds(30));
                    }
                }));

                operation.Succeed();
            }
        }
    }
}
