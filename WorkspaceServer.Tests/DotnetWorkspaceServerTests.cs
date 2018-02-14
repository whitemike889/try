using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pocket;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Dotnet;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public class DotnetWorkspaceServerTests : WorkspaceServerTests
    {
        public DotnetWorkspaceServerTests(ITestOutputHelper output) : base(output)
        {
            RegisterForDisposal(LogEvents.Enrich(log =>
            {
                log(("threadId", Thread.CurrentThread.ManagedThreadId ));
            }));
        }

        protected override WorkspaceRunRequest CreateRunRequestContaining(string text)
        {
            return new WorkspaceRunRequest(
                $@"using System; using System.Linq; using System.Collections.Generic; class Program {{ static void Main() {{ {text}
                    }}
                }}
");
        }

        protected override async Task<IWorkspaceServer> GetWorkspaceServer(
            [CallerMemberName] string testName = null)
        {
            var project = await Create.TestWorkspace(testName);

            var workspaceServer = new DotnetWorkspaceServer(project);

            RegisterForDisposal(workspaceServer);

            await workspaceServer.EnsureInitializedAndNotDisposed();

            return workspaceServer;
        }
    }
}
