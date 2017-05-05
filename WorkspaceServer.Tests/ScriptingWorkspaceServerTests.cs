using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using WorkspaceServer.Tests._Recipes_;
using Xunit;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public class ScriptingWorkspaceServerTests
    {
        private readonly ITestOutputHelper _output;

        public ScriptingWorkspaceServerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        protected IWorkspaceServer GetWorkspaceServer() => new ScriptingWorkspaceServer();

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

            _output.WriteLine(result.ToString());

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
}

Hello.Main();");

            var server = GetWorkspaceServer();

            var result = await server.CompileAndExecute(request);

            _output.WriteLine(result.ToString());

            result.Succeeded.Should().BeTrue();
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

            result.Output
                  .Should()
                  .Contain("Jeff is 20 year(s) old");
        }

        [Fact]
        public void Response_shows_fragment_return_value()
        {
            var request = new BuildAndRunRequest(@"
var person = new { Name = ""Jeff"", Age = 20 };
$""{person.Name} is {person.Age} year(s) old""");

            var server = GetWorkspaceServer();

            var result = server.CompileAndExecute(request).Result;

            _output.WriteLine(result.ToJson());

            result.ReturnValue
                  .Should()
                  .Be("Jeff is 20 year(s) old",
                      "because there is no semicolon on the final line of the fragment");
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
                      s => s.Contains("(2,19): error CS0103: The name \'banana\' does not exist in the current context"));
        }

        [Fact]
        public async Task It_indicates_line_by_line_variable_values()
        {
            var request = new BuildAndRunRequest(@"
string name;
name = ""Jeff"";
name = ""Alice"";");

            var server = GetWorkspaceServer();

            var result = await server.CompileAndExecute(request);

            var states = result.Variables.Single(v => v.Name == "name").States;

            _output.WriteLine(result.ToString());

            states.Select(s => s.LineNumber)
                  .Should()
                  .BeEquivalentTo(2, 3, 4);

            var values = states.Select(s => s.Value).Cast<string>().ToArray();

            _output.WriteLine(new { result }.ToJson());

            values
                .Should()
                .BeEquivalentTo(null, "Jeff", "Alice");
        }

        [Fact]
        public async Task It_indicates_final_variable_values()
        {
            var request = new BuildAndRunRequest(@"
string name;
name = ""Jeff"";
name = ""Alice"";");

            var server = GetWorkspaceServer();

            var result = await server.CompileAndExecute(request);

            var name = result.Variables.Single(v => v.Name == "name").Value;

            _output.WriteLine(result.ToString());

           name.Should().Be("Alice");
        }

        [Fact]
        public async Task Multi_line_console_output_is_captured_correctly()
        {
            var request = new BuildAndRunRequest(@"
Console.WriteLine(1);
Console.WriteLine(2);
Console.WriteLine(3);
Console.WriteLine(4);");

            var server = GetWorkspaceServer();

            var result = await server.CompileAndExecute(request);

            _output.WriteLine(result.ToString());

            result.Output.Should().BeEquivalentTo("1", "2", "3", "4");
        }

        [Fact]
        public async Task Multi_line_console_output_is_captured_correctly_when_an_exception_is_thrown()
        {
            var request = new BuildAndRunRequest(@"
Console.WriteLine(1);
Console.WriteLine(2);
throw new Exception(""oops!"");
Console.WriteLine(3);
Console.WriteLine(4);");

            var server = GetWorkspaceServer();

            var result = await server.CompileAndExecute(request);

            _output.WriteLine(result.ToString());

            result.Output.Should().BeEquivalentTo("1", "2");
        }

        [Fact]
        public async Task When_the_users_code_throws_on_first_line_then_it_is_returned_as_an_exception_property()
        {
            var request = new BuildAndRunRequest(@"throw new Exception(""oops!"");");

            var server = GetWorkspaceServer();

            var result = await server.CompileAndExecute(request);

            _output.WriteLine(result.ToString());

            result.Exception.Should().NotBeNull();
            result.Exception.Should().Contain("oops!");
        }

        [Fact]
        public async Task When_the_users_code_throws_on_subsequent_line_then_it_is_returned_as_an_exception_property()
        {
            var request = new BuildAndRunRequest(@"
throw new Exception(""oops!"");");

            var server = GetWorkspaceServer();

            var result = await server.CompileAndExecute(request);

            _output.WriteLine(result.ToString());

            result.Exception.Should().NotBeNull();
            result.Exception.Should().Contain("oops!");
        }
    }
}
