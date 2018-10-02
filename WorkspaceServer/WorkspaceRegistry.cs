using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Workspaces;

namespace WorkspaceServer
{
    public class WorkspaceRegistry : IEnumerable<WorkspaceBuilder>
    {
        private readonly ConcurrentDictionary<string, WorkspaceBuilder> _workspaceBuilders = new ConcurrentDictionary<string, WorkspaceBuilder>();

        public void Add(string name, Action<WorkspaceBuilder> configure)
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

        public async Task<WorkspaceBuild> Get(string workspaceName, Budget budget = null)
        {
            var build = await _workspaceBuilders.GetOrAdd(
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
                                }).GetWorkspaceBuild(budget);

            await build.EnsureReady(budget);
            
            return build;
        }

        public IEnumerable<WorkspaceInfo> GetRegisteredWorkspaceInfos()
        {
            var workspaceInfos = _workspaceBuilders?.Values.Select(wb => wb.GetWorkpaceInfo()).Where(info => info != null).ToArray() ?? Array.Empty<WorkspaceInfo>();

            return workspaceInfos;
        }

        public static WorkspaceRegistry CreateDefault()
        {
            var registry = new WorkspaceRegistry();

            registry.Add("console",
                         workspace =>
                         {
                             workspace.CreateUsingDotnet("console");
                             workspace.AddPackageReference("Newtonsoft.Json");
                         });

            registry.Add("nodatime.api",
                         workspace =>
                         {
                             workspace.CreateUsingDotnet("console");
                             workspace.AddPackageReference("NodaTime", "2.3.0");
                             workspace.AddPackageReference("NodaTime.Testing", "2.3.0");
                         });

            registry.Add("aspnet.webapi",
                         workspace =>
                         {
                             workspace.CreateUsingDotnet("webapi");
                             workspace.RequiresPublish = true;
                         });

            registry.Add("instrumented",
                         workspace =>
                         {
                             workspace.CreateUsingDotnet("console");
                             workspace.AddPackageReference("Newtonsoft.Json");
                         });

            registry.Add("xunit",
                         workspace =>
                         {
                             workspace.CreateUsingDotnet("xunit", "tests");
                             workspace.AddPackageReference("Newtonsoft.Json");
                             workspace.DeleteFile("UnitTest1.cs");
                         });

            return registry;
        }

        public IEnumerator<WorkspaceBuilder> GetEnumerator() =>
            _workspaceBuilders.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
