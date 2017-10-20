using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Pocket;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Local;
using Xunit;
using Xunit.Abstractions;
using static Pocket.Logger<WorkspaceServer.Tests.LocalWorkspaceServerTests>;

namespace WorkspaceServer.Tests
{
    public class LocalWorkspaceServerTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public LocalWorkspaceServerTests(ITestOutputHelper output)
        {
            disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));
        }

        protected IWorkspaceServer GetWorkspaceServer()
        {
            var directoryName = $"{DateTime.Now:yyyy.MM.dd-hh.mm.ss}";

            var directory = new DirectoryInfo(directoryName);

            return new LocalWorkspaceServer(directory);
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public async Task Response_indicates_when_compile_is_successful_and_signature_is_like_a_console_app()
        {
            var request = new RunRequest(@"
using System;

public static class Hello 
{
    public static void Main() 
    {
    }
}
");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            Log.Info(string.Join("\n", result.Output));

            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task Response_shows_program_output_when_compile_is_successful_and_signature_is_like_a_console_app()
        {
            var request = new RunRequest(@"
using System;

public static class Hello 
{
    public static void Main() 
    { 
        Console.WriteLine(""Hello there!"");
    } 
}");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.Output.Should().Contain("Hello there!");
        }

        [Fact]
        public async Task Response_shows_program_output_when_compile_is_successful_and_signature_is_a_fragment_containing_console_output()
        {
            var request = new RunRequest(@"
var person = new { Name = ""Jeff"", Age = 20 };
var s = $""{person.Name} is {person.Age} year(s) old"";
Console.WriteLine(s);");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.Output.Should().Contain("Jeff is 20 year(s) old");
        }

        [Fact]
        public async Task Response_indicates_when_compile_is_unsuccessful()
        {
            var request = new RunRequest(@"
Console.WriteLine(banana);");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.Succeeded.Should().BeFalse();

            Log.Info(string.Join("\n", result.Output));

            result.Output
                  .Should()
                  .Contain(
                      s => s.Contains("(3,19): error CS0103: The name \'banana\' does not exist in the current context"));
        }

        [Fact(Skip = "Later")]
        public async Task Response_indicates_when_execution_times_out()
        {
            var request = new RunRequest(@"
while (false)
{
    Console.WriteLine(""still going..."");
}");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.Succeeded.Should().BeFalse();

            result.Exception
                  .Should()
                  .Contain("something something something");
        }

        [Fact]
        public async Task Code_can_be_updated_and_rerun_in_the_same_workspace()
        {
            var server = GetWorkspaceServer();

            var result = await server.Run(
                             new RunRequest(@"Console.WriteLine(i don't compile!);"));

            result.Output.Should().Contain(line => line.Contains("Syntax error"));

            result = await server.Run(
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
