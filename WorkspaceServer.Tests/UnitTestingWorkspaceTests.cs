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
                "PASSED*(*s)",
                "  tests*(*s)",
                "    UnitTest1*(*s)",
                "      Test1*(*s)",
                "SUMMARY:",
                "Passed: 1, Failed: 0, Not run: 0"
            );
        }

        [Fact(Skip = "wip")]
        public async Task Subsequent_runs_update_test_output()
        {
            // FIX: (Subsequent_runs_update_test_output) enable this test
            var (runner, workspace) = await GetRunnerAndWorkspace();

            var workspaceModel = Workspace.FromDirectory(
                workspace.Directory,
                workspace.Name);

            workspaceModel = workspaceModel
                             .ReplaceFile(
                                 "UnitTest1.cs",
                                 @"
using System; 
using Xunit;

public class Tests 
{
#region facts
    [Fact] public void passing() {  }
#endregion

}")
                             .ReplaceBuffer(
                                 "UnitTest1.cs",
                                 "");

            var runResult = await runner.Run(workspaceModel);

            Log.Info("Output: {output}", runResult.Output);

            runResult.Output.ShouldMatch(
                "PASSED*(*s)",
                "  tests*(*s)",
                "    UnitTest1*(*s)",
                "      Test1*(*s)",
                "SUMMARY:",
                "Passed: 1, Failed: 0, Not run: 0"
            );

            // TODO (Subsequent_runs_update_test_output) write test
            throw new NotImplementedException("Test Subsequent_runs_update_test_output is not written yet.");
        }

        protected async Task<(ICodeRunner server, WorkspaceBuild workspace )> GetRunnerAndWorkspace(
            [CallerMemberName] string testName = null)
        {
            var workspace = await Create.XunitWorkspaceCopy(testName);

            var server = new RoslynWorkspaceServer(workspace);

            return (server, workspace);
        }
    }
}
