using System.Threading.Tasks;
using FluentAssertions;
using MLS.Protocol;
using MLS.Protocol.Execution;
using WorkspaceServer.Servers.Scripting;
using WorkspaceServer.Packaging;
using Xunit;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public class ScriptingWorkspaceServerDiagnosticsTests : WorkspaceServerTestsCore
    {
        public ScriptingWorkspaceServerDiagnosticsTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override ILanguageService GetLanguageService(string testName = null) =>
            new ScriptingWorkspaceServer();

        protected override Task<(ICodeRunner runner, Package workspace)> GetRunnerAndWorkspaceBuild(string testName = null)
        {
            return Task.FromResult(((ICodeRunner)new ScriptingWorkspaceServer(), new Package("script")));
        }

        [Fact]
        public async Task Get_diagnostics()
        {
            var code = @"addd";
            var (processed, markLocation) = CodeManipulation.ProcessMarkup(code);

            var ws = new Workspace(buffers: new[] { new Workspace.Buffer("", processed, markLocation) });
            var request = new WorkspaceRequest(ws, activeBufferId: "");
            var server = GetLanguageService();
            var result = await server.GetDiagnostics(request);
            result.Diagnostics.Should().NotBeEmpty();
            result.Diagnostics.Should().Contain(diagnostics => diagnostics.Message == "(1,1): error CS0103: The name \'addd\' does not exist in the current context");
        }
    }
}