using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;

namespace WorkspaceServer
{
    public class WorkspaceRegistry : IDisposable
    {
        private readonly ConcurrentDictionary<string, WorkspaceBuilder> _workspaceBuilders = new ConcurrentDictionary<string, WorkspaceBuilder>();

        private readonly ConcurrentDictionary<string, WorkspaceBuild> _workspaceServers = new ConcurrentDictionary<string, WorkspaceBuild>();

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

            var options = new WorkspaceBuilder(this, name);
            configure(options);
            _workspaceBuilders.TryAdd(name, options);
        }

        public async Task<WorkspaceBuild> GetWorkspace(string workspaceName, Budget budget = null)
        {
            var workspace = await _workspaceBuilders.GetOrAdd(
                                workspaceName,
                                name =>
                                {
                                    var directory = new DirectoryInfo(
                                        Path.Combine(
                                            WorkspaceBuild.DefaultWorkspacesDirectory.FullName, workspaceName));

                                    if (directory.Exists)
                                    {
                                        return new WorkspaceBuilder(this, name);
                                    }

                                    throw new ArgumentException($"Workspace named \"{name}\" not found.");
                                }).GetWorkspace(budget);

            await workspace.EnsureReady(budget);

            return workspace;
        }

        public void Dispose()
        {
            foreach (var workspaceServer in _workspaceServers.Values.OfType<IDisposable>())
            {
                workspaceServer.Dispose();
            }
        }

        public IEnumerable<WorkspaceInfo> GetRegisterWorkspaceInfos()
        {
            var workspaceInfos = _workspaceBuilders?.Values.Select(wb => wb.GetWorkpaceInfo()).Where(info => info != null).ToArray() ?? Array.Empty<WorkspaceInfo>();

            return workspaceInfos;
        }

        public static WorkspaceRegistry CreateDefault()
        {
            var registry = new WorkspaceRegistry();

            registry.AddWorkspace("console",
                                  workspace =>
                                  {
                                      workspace.CreateUsingDotnet("console");
                                      workspace.AddPackageReference("Newtonsoft.Json");
                                  });

            registry.AddWorkspace("nodatime.api",
                                  workspace =>
                                  {
                                      workspace.CreateUsingDotnet("console");
                                      workspace.AddPackageReference("NodaTime", "2.3.0");
                                      workspace.AddPackageReference("NodaTime.Testing", "2.3.0");
                                  });

            registry.AddWorkspace("aspnet.webapi",
                                  workspace =>
                                  {
                                      workspace.CreateUsingDotnet("webapi");
                                      workspace.RequiresPublish = true;
                                  });

            registry.AddWorkspace("instrumented",
                                  workspace =>
                                  {
                                      workspace.CreateUsingDotnet("console");
                                  });

            registry.AddWorkspace("xunit",
                                  workspace =>
                                  {
                                      workspace.CreateUsingDotnet("xunit", "tests");
                                  });

            return registry;
        }
    }
}
