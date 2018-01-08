using System;
using FluentAssertions;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Pocket;
using Recipes;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Scripting;
using Xunit;
using Xunit.Abstractions;
using static Pocket.Logger<WorkspaceServer.Tests.WorkspaceServerTests>;

namespace WorkspaceServer.Tests
{
    public class ScriptingWorkspaceServerTests : WorkspaceServerTests
    {
        public ScriptingWorkspaceServerTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override IWorkspaceServer GetWorkspaceServer(
            int defaultTimeoutInSeconds = 10,
            [CallerMemberName] string testName = null) => new ScriptingWorkspaceServer(defaultTimeoutInSeconds);

        [Fact]
        public void Response_shows_fragment_return_value()
        {
            var request = new RunRequest(@"
var person = new { Name = ""Jeff"", Age = 20 };
$""{person.Name} is {person.Age} year(s) old""");

            var server = GetWorkspaceServer();

            var result = server.Run(request).Result;

            Log.Trace(result.ToJson());

            result.ShouldBeEquivalentTo(new
            {
                Succeeded = true,
                Output = new string[] { },
                Exception = (string) null,
                ReturnValue = "Jeff is 20 year(s) old",
            }, config => config.ExcludingMissingMembers());
        }

        [Fact]
        public async Task Response_indicates_when_compile_is_unsuccessful()
        {
            var request = new RunRequest(@"
Console.WriteLine(banana);");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.ShouldBeEquivalentTo(new
            {
                Succeeded = false,
                Output = new string[] { "(2,19): error CS0103: The name \'banana\' does not exist in the current context" },
                Exception = (string) null, // we already display the error in Output
            }, config => config.ExcludingMissingMembers());
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
        public async Task Get_completion_for_console()
        {
            var request = new CompletionRequest("Console.", position: 8);

            var server = GetWorkspaceServer();

            var result = await server.GetCompletionList(request);

            result.Items.Should().ContainSingle(item => item.DisplayText == "WriteLine");
        }

        [Fact]
        public async Task Additional_using_statements_from_request_are_passed_to_scripting_when_running_snippet()
        {
            var request = new RunRequest(@"
using System;

public static class Hello
{
    public static void Main()
    {
        Thread.Sleep(1);
        Console.WriteLine(""Hello there!"");
    }
}

Hello.Main();",
                                         usings: new[] { "System.Threading" });

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.ShouldBeEquivalentTo(new
            {
                Succeeded = true,
                Output = new[] { "Hello there!" },
                Exception = (string) null,
            }, config => config.ExcludingMissingMembers());
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

            result.ShouldSucceedWithNoOutput();
        }
    }
}
