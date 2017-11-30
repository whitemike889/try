using System;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Pocket;
using Recipes;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Scripting;
using Xunit;
using Xunit.Abstractions;
using static Pocket.Logger<WorkspaceServer.Tests.ScriptingWorkspaceServerTests>;

namespace WorkspaceServer.Tests
{
    [CollectionDefinition("Scripting Workspace Server Tests", DisableParallelization=true)]
    public class ScriptingWorkspaceServerTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public ScriptingWorkspaceServerTests(ITestOutputHelper output)
        {
            disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));
        }

        protected IWorkspaceServer GetWorkspaceServer() => new ScriptingWorkspaceServer();

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

            Log.Trace(result.ToString());

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
}

Hello.Main();");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            Log.Trace(result.ToString());

            result.Succeeded.Should().BeTrue();
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

            result.Succeeded
                  .Should()
                  .BeTrue();
            result.Output
                  .Should()
                  .Contain("Jeff is 20 year(s) old");
        }

        [Fact]
        public void Response_shows_fragment_return_value()
        {
            var request = new RunRequest(@"
var person = new { Name = ""Jeff"", Age = 20 };
$""{person.Name} is {person.Age} year(s) old""");

            var server = GetWorkspaceServer();

            var result = server.Run(request).Result;

            Log.Trace(result.ToJson());

            result.ReturnValue
                  .Should()
                  .Be("Jeff is 20 year(s) old",
                      "because there is no semicolon on the final line of the fragment");
        }

        [Fact]
        public async Task Response_indicates_when_compile_is_unsuccessful()
        {
            var request = new RunRequest(@"
Console.WriteLine(banana);");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.Succeeded.Should().BeFalse();

            Log.Trace(string.Join("\n", result.Output));

            result.Output
                  .Should()
                  .Contain(
                      s => s.Contains("(2,19): error CS0103: The name \'banana\' does not exist in the current context"));
        }

        [Fact]
        public async Task When_compile_is_unsuccessful_then_no_exceptions_are_shown()
        {
            var request = new RunRequest(@"
Console.WriteLine(banana);");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.Succeeded.Should().BeFalse();

            Log.Trace(string.Join("\n", result.Output));

            result.Exception
                  .Should()
                  .BeNull($"we already display compilation errors in {nameof(RunResult.Output)}");
        }

        [Fact]
        public async Task It_indicates_line_by_line_variable_values()
        {
            var request = new RunRequest(@"
string name;
name = ""Jeff"";
name = ""Alice"";");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            var states = result.Variables.Single(v => v.Name == "name").States;

            Log.Trace(result.ToString());

            states.Select(s => s.LineNumber)
                  .Should()
                  .BeEquivalentTo(2, 3, 4);

            var values = states.Select(s => s.Value).Cast<string>().ToArray();

            Log.Trace(new { result }.ToJson());

            values
                .Should()
                .BeEquivalentTo(null, "Jeff", "Alice");
        }

        [Fact]
        public async Task It_indicates_final_variable_values()
        {
            var request = new RunRequest(@"
string name;
name = ""Jeff"";
name = ""Alice"";");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            var name = result.Variables.Single(v => v.Name == "name").Value;

            Log.Trace(result.ToString());

            name.Should().Be("Alice");
        }

        [Fact]
        public async Task Multi_line_console_output_is_captured_correctly()
        {
            var request = new RunRequest(@"
Console.WriteLine(1);
Console.WriteLine(2);
Console.WriteLine(3);
Console.WriteLine(4);");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            Log.Trace(result.ToString());

            result.Output.Should().BeEquivalentTo("1", "2", "3", "4");
        }

        [Fact]
        public async Task Multi_line_console_output_is_captured_correctly_when_an_exception_is_thrown()
        {
            var request = new RunRequest(@"
Console.WriteLine(1);
Console.WriteLine(2);
throw new Exception(""oops!"");
Console.WriteLine(3);
Console.WriteLine(4);");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            Log.Trace(result.ToString());

            result.Output.Should().BeEquivalentTo("1", "2");
        }

        [Fact]
        public async Task When_the_users_code_throws_on_first_line_then_it_is_returned_as_an_exception_property()
        {
            var request = new RunRequest(@"throw new Exception(""oops!"");");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            Log.Trace(result.ToString());

            result.Exception.Should().NotBeNull();
            result.Exception.Should().Contain("oops!");
        }

        [Fact]
        public async Task When_the_users_code_throws_on_subsequent_line_then_it_is_returned_as_an_exception_property()
        {
            var request = new RunRequest(@"
throw new Exception(""oops!"");");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            Log.Trace(result.ToString());

            result.Exception.Should().NotBeNull();
            result.Exception.Should().Contain("oops!");
        }

        [Fact]
        public async Task Get_completion_for_console()
        {
            var request = new CompletionRequest("Console.", position: 8);

            var server = GetWorkspaceServer();

            var result = await server.GetCompletionList(request);

            result.Items.Should().ContainSingle(item => item.DisplayText == "WriteLine");
        }

        [Fact]
        public async Task Response_indicates_when_execution_times_out()
        {
            var request = new RunRequest(@"while (true) {  }");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.Succeeded.Should().BeFalse();

            result.Exception
                  .Should()
                  .StartWith("System.TimeoutException");
        }

        [Fact]
        public async Task When_a_public_void_Main_with_no_parameters_is_present_it_is_invoked()
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

            Log.Trace(result.ToString());

            result.Succeeded.Should().BeTrue();
            result.Output.Should().Contain("Hello there!");
        }

        [Fact]
        public async Task When_a_public_void_Main_with_parameters_is_present_it_is_invoked()
        {
            var request = new RunRequest(@"
using System;

public static class Hello
{
    public static void Main(params string[] args)
    {
        Console.WriteLine(""Hello there!"");
    }
}");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            Log.Trace(result.ToString());

            result.Succeeded.Should().BeTrue();
            result.Output.Should().Contain("Hello there!");
        }

        [Fact]
        public async Task When_a_public_void_Main_with_non_string_parameters_is_present_it_is_not_invoked()
        {
            var request = new RunRequest(@"
using System;

public static class Hello
{
    public static void Main(params int[] args)
    {
        Console.WriteLine(""Hello there!"");
    }
}");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            Log.Trace(result.ToString());

            result.Succeeded.Should().BeTrue();
            result.Output.Should().BeEmpty();
        }

        [Fact]
        public async Task When_an_internal_void_Main_with_no_parameters_is_present_it_is_invoked()
        {
            var request = new RunRequest(@"
using System;

public static class Hello
{
    static void Main()
    {
        Console.WriteLine(""Hello there!"");
    }
}");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            Log.Trace(result.ToString());

            result.Succeeded.Should().BeTrue();
            result.Output.Should().Contain("Hello there!");
        }

        [Fact]
        public async Task When_an_internal_void_Main_with_parameters_is_present_it_is_invoked()
        {
            var request = new RunRequest(@"
using System;

public static class Hello
{
    static void Main(string[] args)
    {
        Console.WriteLine(""Hello there!"");
    }
}");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            Log.Trace(result.ToString());

            result.Succeeded.Should().BeTrue();
            result.Output.Should().Contain("Hello there!");
        }


    }
}
