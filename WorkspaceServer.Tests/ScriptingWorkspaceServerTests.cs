using FluentAssertions;
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

        protected override WorkspaceRequest CreateRunRequestContaining(string text) => new WorkspaceRequest(new Workspace(text));

        protected override Task<IWorkspaceServer> GetWorkspaceServer(
            [CallerMemberName] string testName = null) =>
            Task.FromResult<IWorkspaceServer>(new ScriptingWorkspaceServer());

        [Fact]
        public async Task Response_shows_fragment_return_value()
        {
            var workspace = new Workspace(@"
var person = new { Name = ""Jeff"", Age = 20 };
$""{person.Name} is {person.Age} year(s) old""");
            var request = new WorkspaceRequest(workspace);
            var server = await GetWorkspaceServer();

            var result = await server.Run(request);

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
            var workspace = new Workspace(@"
Console.WriteLine(banana);");
            var request = new WorkspaceRequest(workspace);
            var server = await GetWorkspaceServer();

            var result = await server.Run(request);

            result.ShouldBeEquivalentTo(new
            {
                Succeeded = false,
                Output = new string[] { "(2,19): error CS0103: The name \'banana\' does not exist in the current context" },
                Exception = (string) null, // we already display the error in Output
            }, config => config.ExcludingMissingMembers());
        }

        [Fact]
        public async Task Get_completion_for_console()
        {
            var request = new CompletionRequest("Console.", position: 8);

            var server = await GetWorkspaceServer();

            var result = await server.GetCompletionList(request);

            result.Items.Should().ContainSingle(item => item.DisplayText == "WriteLine");
        }

        [Fact]
        public async Task Additional_using_statements_from_request_are_passed_to_scripting_when_running_snippet()
        {
            var workspace = new Workspace(@"
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
            var request = new WorkspaceRequest(workspace);
            var server = await GetWorkspaceServer();

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
            var workspace = new Workspace(@"
using System;

public static class Hello
{
    public static void Main(params int[] args)
    {
        Console.WriteLine(""Hello there!"");
    }
}");
            var request = new WorkspaceRequest(workspace);
            var server = await GetWorkspaceServer();

            var result = await server.Run(request);

            result.ShouldSucceedWithNoOutput();
        }

        [Fact]
        public async Task CS7022_not_reported_for_main_in_global_script_code()
        {
            var workspace = new Workspace(@"
using System;

public static class Hello
{
    public static void Main()
    {
        Console.WriteLine(""Hello there!"");
    }
}");
            var request = new WorkspaceRequest(workspace);
            var server = await GetWorkspaceServer();

            var result = await server.Run(request);

            result.Diagnostics.Should().NotContain(d => d.Id == "CS7022");
        }

        [Fact]
        public async Task When_using_buffers_they_are_inlined()
        {
            var fileCode = @"using System;

public static class Hello
{
    static void Main(string[] args)
    {
        #region toReplace
        Console.WriteLine(""Hello world!"");
        #endregion
    }
}";
            var workspace = new Workspace(
                workspaceType: "script",
                files: new[] { new Workspace.File("Main.cs", fileCode) },
                buffers: new[] { new Workspace.Buffer(@"Main.cs@toReplace", @"Console.WriteLine(""Hello there!"");", 0) });
            var request = new WorkspaceRequest(workspace);
            var server = await GetWorkspaceServer();

            var result = await server.Run(request);

            result.ShouldSucceedWithOutput("Hello there!");
        }

        [Fact]
        public async Task When_using_buffers_they_are_inlined_and_warnings_are_aligned()
        {
            var fileCode = @"using System;

public static class Hello
{
    static void Main(string[] args)
    {
        #region toReplace
        Console.WriteLine(""Hello world!"");
        #endregion
    }
}";
            var workspace = new Workspace(
                workspaceType: "script",
                files: new[] { new Workspace.File("Main.cs", fileCode) },
                buffers: new[] { new Workspace.Buffer(@"Main.cs@toReplace", @"Console.WriteLine(banana);", 0) });

            var server = await GetWorkspaceServer();
            var request = new WorkspaceRequest(workspace);
            var result = await server.Run(request);

            result.ShouldBeEquivalentTo(new
            {
                Succeeded = false,
                Output = new[] { "(1,19): error CS0103: The name \'banana\' does not exist in the current context" },
                Exception = (string)null, 
            }, config => config.ExcludingMissingMembers());
            
        }
    }
}
