using FluentAssertions;
using MLS.Agent.Tools;
using MLS.Protocol;
using MLS.Protocol.Execution;
using Recipes;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Threading.Tasks;
using WorkspaceServer.Packaging;
using WorkspaceServer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace MLS.Agent.Tests
{
    public class WorkspaceDiscoveryTests : ApiViaHttpTestsBase
    {
        public WorkspaceDiscoveryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task Local_tool_workspace_can_be_discovered()
        {
            var console = new TestConsole();
            await InstallLocalTool(console);

            var output = Guid.NewGuid().ToString();
            var requestJson = Create.SimpleWorkspaceRequestAsJson(output, "BasicConsoleApp");

            var response = await CallRun(requestJson);
            var result = await response
                                .EnsureSuccess()
                                .DeserializeAs<RunResult>();

            result.Succeeded.Should().BeTrue();

            result.ShouldSucceedWithOutput(output);
        }

        [Fact]
        public async Task Project_file_path_workspace_can_be_discovered_and_run_with_buffer_inlining()
        {
            var csproj = TestAssets.SampleConsole.GetFiles("*.csproj")[0];
            var programCs = TestAssets.SampleConsole.GetFiles("*.cs")[0];

            var output = Guid.NewGuid().ToString();
            var ws = new Workspace(
                files: new[] {  new Workspace.File(programCs.FullName, "") },
                buffers: new[] { new Workspace.Buffer(new BufferId(programCs.FullName, "theregion"), $"Console.WriteLine(\"{output}\");") },
                workspaceType: csproj.FullName);

            var requestJson = new WorkspaceRequest(ws, requestId: "TestRun").ToJson();

            var response = await CallRun(requestJson);
            var result = await response
                                .EnsureSuccess()
                                .DeserializeAs<RunResult>();

            result.Succeeded.Should().BeTrue();

            result.ShouldSucceedWithOutput(output);
        }

        private async Task InstallLocalTool(IConsole console)
        {
            using (var dir = DisposableDirectory.Create())
            {
                await PackageCommand.Do(TestAssets.SampleConsole, dir.Directory, console);
                await InstallCommand.Do("BasicConsoleApp", dir.Directory.FullName, console);
            }
        }
    }
}
