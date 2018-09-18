using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Clockwise;
using FluentAssertions;
using Pocket;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Roslyn;
using WorkspaceServer.Workspaces;
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
                new WorkspaceRequest(
                                Workspace.FromDirectory(
                                    workspace.Directory,
                                    workspace.Name)));

            Log.Info("Output: {output}", runResult.Output);

            runResult.Output.ShouldMatch(
                "PASSED*(*s)",
                "  tests*(*s)",
                "    UnitTest1*(*s)",
                "      Test1*(*s)",
                "",
                "SUMMARY:",
                "Passed: 1, Failed: 0, Not run: 0",
                ""
            );
        }

        [Fact]
        public async Task Subsequent_runs_update_test_output()
        {
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

namespace MyUnitTestNamespace
{
    public class MyUnitTestClass 
    {
#region facts
#endregion
    }
}")
                             .RemoveBuffer("UnitTest1.cs")
                             .AddBuffer("UnitTest1.cs@facts", "[Fact] public void passing() {  }");

            var runResult1 = await runner.Run(new WorkspaceRequest(workspaceModel));

            Log.Info("Output1: {output}", runResult1.Output);

            runResult1.Output.ShouldMatch(
                "PASSED*(*s)",
                "  MyUnitTestNamespace*(*s)",
                "    MyUnitTestClass*(*s)",
                "      passing*(*s)",
                "",
                "SUMMARY:",
                "Passed: 1, Failed: 0, Not run: 0"
            );

            workspaceModel = workspaceModel.ReplaceBuffer(
                id: "UnitTest1.cs@facts",
                text: @"
[Fact] public void still_passing() {  } 
[Fact] public void failing() => throw new Exception(""oops!"");
");

            var runResult2 = await runner.Run(new WorkspaceRequest(workspaceModel));

            Log.Info("Output2: {output}", runResult2.Output);

            runResult2.Output.ShouldMatch(
                "PASSED*(*s)",
                "  MyUnitTestNamespace*(*s)",
                "    MyUnitTestClass*(*s)",
                "      still_passing*(*s)",
                "",
                "FAILED*(*s)",
                "  MyUnitTestNamespace*(*s)",
                "    MyUnitTestClass*(*s)",
                "      failing*(*s)",
                "        System.Exception : oops!",
                "        Stack trace:",
                "           at MyUnitTestNamespace.MyUnitTestClass.failing()",
                "",
                "SUMMARY:",
                "Passed: 1, Failed: 1, Not run: 0"
            );
        }

        [Fact]
        public async Task RunResult_does_not_show_exception_for_test_failures()
        {
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

namespace MyUnitTestNamespace
{
    public class MyUnitTestClass 
    {
#region facts
#endregion
    }
}")
                             .RemoveBuffer("UnitTest1.cs")
                             .AddBuffer("UnitTest1.cs@facts", "[Fact] public void failing() => throw new Exception(\"oops!\");");

            var runResult = await runner.Run(new WorkspaceRequest(workspaceModel));

            Log.Info("Output: {output}", runResult.Output);

            runResult.Exception.Should().BeNullOrEmpty();
        }

        protected async Task<(ICodeRunner server, WorkspaceBuild workspace)> GetRunnerAndWorkspace(
            [CallerMemberName] string testName = null)
        {
            var workspace = await Create.XunitWorkspaceCopy(testName);

            var server = new RoslynWorkspaceServer(workspace);

            return (server, workspace);
        }
    }
}
