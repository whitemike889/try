using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public abstract class WorkspaceServerTests
    {
        protected abstract IWorkspaceServer GetWorkspaceServer();

        protected readonly ITestOutputHelper _output;

        protected WorkspaceServerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Response_indicates_when_compile_is_successful_and_signature_is_like_a_console_app()
        {
            var request = new BuildAndRunRequest(@"
using System;

public static class Hello 
{
    public static void Main() 
    {
    } 
}
");

            var server = GetWorkspaceServer();

            var result = await server.CompileAndExecute(request);

            _output.WriteLine(string.Join("\n", result.Output));

            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task Response_shows_program_output_when_compile_is_successful_and_signature_is_like_a_console_app()
        {
            var request = new BuildAndRunRequest(@"
using System;

public static class Hello 
{
    public static void Main() 
    { 
        Console.WriteLine(""Hello there!"");
    } 
}");

            var server = GetWorkspaceServer();

            var result = await server.CompileAndExecute(request);

            result.Output.Should().Contain("Hello there!");
        }

        [Fact]
        public async Task Response_shows_program_output_when_compile_is_successful_and_signature_is_a_fragment_containing_console_output()
        {
            var request = new BuildAndRunRequest(@"
var person = new { Name = ""Jeff"", Age = 20 };
var s = $""{person.Name} is {person.Age} year(s) old"";
Console.WriteLine(s);");

            var server = GetWorkspaceServer();

            var result = await server.CompileAndExecute(request);

            result.Output.Should().Contain("Jeff is 20 year(s) old");
        }

        [Fact]
        public async Task Response_indicates_when_compile_is_unsuccessful()
        {
            var request = new BuildAndRunRequest(@"
Console.WriteLine(banana);");

            var server = GetWorkspaceServer();

            var result = await server.CompileAndExecute(request);

            result.Succeeded.Should().BeFalse();

            _output.WriteLine(string.Join("\n", result.Output));

            result.Output
                  .Should()
                  .Contain(
                      s => s.Contains("(3,19): error CS0103: The name \'banana\' does not exist in the current context"));
        }
    }
}