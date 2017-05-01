using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public class DotDotnetWorkspaceServerTests : WorkspaceServerTests
    {
        protected override IWorkspaceServer GetWorkspaceServer()
        {
            return new DotDotnetWorkspaceServer();
        }

        public DotDotnetWorkspaceServerTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}