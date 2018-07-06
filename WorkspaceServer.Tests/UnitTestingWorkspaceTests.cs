using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;
using Pocket;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Roslyn;
using Xunit;
using Xunit.Abstractions;
using static Pocket.Logger;

namespace WorkspaceServer.Tests
{
    public class UnitTestingWorkspaceTests : IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public UnitTestingWorkspaceTests(ITestOutputHelper output)
        {
            _disposables.Add(output.SubscribeToPocketLogger());
            _disposables.Add(VirtualClock.Start());
        }

        public void Dispose() => _disposables.Dispose();

        [Fact]
        public async Task Run_executes_unit_tests_and_prints_test_results_to_output()
        {
            var (runner, workspace) = await GetRunnerAndWorkspace();

            var runResult = await runner.Run(
                                Workspace.FromDirectory(
                                    workspace.Directory,
                                    workspace.Name));

            Log.Info("Output: {output}", runResult.Output);

            runResult.Output.ShouldMatch(
                "PASSED",
                "*NAME*RESULT*SECONDS",
                "*tests.UnitTest1.Test1*s*Run_executes_unit_tests_and_prints_test_results_to_output*",
                "SUMMARY:",
                "*Passed: 1, Failed: 0, Not run: 0"
            );
        }

        protected async Task<(ICodeRunner server, Workspace workspace )> GetRunnerAndWorkspace(
        protected async Task<(ICodeRunner server, WorkspaceBuild workspace )> GetRunnerAndWorkspace(
            [CallerMemberName] string testName = null)
        {
            var workspace = await Create.XunitWorkspaceCopy(testName);

            var server = new RoslynWorkspaceServer(workspace);

            return (server, workspace);
        }
    }
}
