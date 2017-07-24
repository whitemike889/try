using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Local;
using Xunit;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public class LocalWorkspaceServerTests : WorkspaceServerTests
    {
        protected override IWorkspaceServer GetWorkspaceServer()
        {
            var directoryName = DateTime.Now.ToString("yyyy.MM.dd-hh.mm.ss");

            var directory = new DirectoryInfo(directoryName);

            return new LocalWorkspaceServer(directory);
        }

        public LocalWorkspaceServerTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task Code_can_be_updated_and_rerun_in_the_same_workspace()
        {
            var server = GetWorkspaceServer();

            ProcessResult result;

            result = await server.CompileAndExecute(
                         new RunRequest(@"Console.WriteLine(i don't compile!);"));

            result.Output.Should().Contain(line => line.Contains("Syntax error"));

            result = await server.CompileAndExecute(
                         new RunRequest(@"
using System;

public static class Hello 
{
    public static void Main() 
    { 
        Console.WriteLine(""i do compile!"");
    } 
}"));

            result.Output.Should().Contain("i do compile!");
        }
    }
}
