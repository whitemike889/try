using System;
using System.Runtime.CompilerServices;
using WorkspaceServer.Servers.OmniSharp;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public class DotnetWorkspaceServerTests : WorkspaceServerTests
    {
        public DotnetWorkspaceServerTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override IWorkspaceServer GetWorkspaceServer(
            int defaultTimeoutInSeconds = 10,
            [CallerMemberName] string testName = null)
        {
            var project = Create.TempProject(testName);

            var workspaceServer = new DotnetWorkspaceServer(project);

            RegisterForDisposal(workspaceServer);

            return workspaceServer;
        }
    }
}
