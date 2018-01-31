using System;
using FluentAssertions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using MLS.Agent.Tools;
using Pocket;
using WorkspaceServer.Models.Execution;
using Xunit;
using Xunit.Abstractions;
using static Pocket.Logger<WorkspaceServer.Tests.WorkspaceServerTests>;

namespace WorkspaceServer.Tests
{
    public abstract class WorkspaceServerTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        protected abstract IWorkspaceServer GetWorkspaceServer(
            [CallerMemberName] string testName = null);

        protected abstract RunRequest CreateRunRequestContaining(string text);

        public void Dispose() => disposables.Dispose();

        protected void RegisterForDisposal(IDisposable disposable) => disposables.Add(disposable);

        [Fact]
        public async Task Diagnostic_logs_do_not_show_up_in_captured_console_output()
        {
            using (LogEvents.Subscribe(e => Console.WriteLine(e.ToLogString())))
            {
                var server = GetWorkspaceServer();

                var result = await server.Run(CreateRunRequestContaining("Console.WriteLine(\"hi!\");"));

                result.Output.Should().BeEquivalentTo("hi!");
            }
        }

        protected WorkspaceServerTests(ITestOutputHelper output)
        {
            disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));
        }

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

            result.ShouldSucceedWithNoOutput();
        }

        [Fact]
        public async Task Response_shows_program_output_when_compile_is_successful_and_signature_is_like_a_console_app()
        {
            var output = nameof(Response_shows_program_output_when_compile_is_successful_and_signature_is_like_a_console_app);

            var request = new RunRequest($@"
using System;

public static class Hello
{{
    public static void Main()
    {{
        Console.WriteLine(""{output}"");
    }}
}}");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.ShouldSucceedWithOutput(output);
        }

        [Fact]
        public async Task Response_shows_program_output_when_compile_is_successful_and_signature_is_a_fragment_containing_console_output()
        {
            var request = CreateRunRequestContaining(@"
var person = new { Name = ""Jeff"", Age = 20 };
var s = $""{person.Name} is {person.Age} year(s) old"";
Console.Write(s);");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.ShouldSucceedWithOutput("Jeff is 20 year(s) old");
        }

        [Fact]
        public async Task When_compile_is_unsuccessful_then_no_exceptions_are_shown()
        {
            var request = CreateRunRequestContaining(@"
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
        public async Task Multi_line_console_output_is_captured_correctly()
        {
            var request = CreateRunRequestContaining(@"
Console.WriteLine(1);
Console.WriteLine(2);
Console.WriteLine(3);
Console.WriteLine(4);");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.ShouldSucceedWithOutput("1", "2", "3", "4");
        }

        [Fact]
        public async Task Multi_line_console_output_is_captured_correctly_when_an_exception_is_thrown()
        {
            var request = CreateRunRequestContaining(@"
Console.WriteLine(1);
Console.WriteLine(2);
throw new Exception(""oops!"");
Console.WriteLine(3);
Console.WriteLine(4);");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.ShouldSucceedWithExceptionContaining(
                "System.Exception: oops!",
                output: new string[] { "1", "2" });
        }

        [Fact]
        public async Task When_the_users_code_throws_on_first_line_then_it_is_returned_as_an_exception_property()
        {
            var request = CreateRunRequestContaining(@"throw new Exception(""oops!"");");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.ShouldSucceedWithExceptionContaining("System.Exception: oops!");
        }

        [Fact]
        public async Task When_the_users_code_throws_on_subsequent_line_then_it_is_returned_as_an_exception_property()
        {
            var request = CreateRunRequestContaining(@"
throw new Exception(""oops!"");");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.ShouldSucceedWithExceptionContaining("System.Exception: oops!");
        }

        [Fact]
        public async Task Response_indicates_when_execution_times_out()
        {
            var request = CreateRunRequestContaining(@"while (true) {  }");

            var server = GetWorkspaceServer();

            var result = await server.Run(request, Clock.Current.CreateCancellationToken(1.Seconds()));

            result.ShouldFailWithExceptionContaining("System.TimeoutException: The operation has timed out.");
        }

        [Fact]
        public async Task When_a_public_void_Main_with_no_parameters_is_present_it_is_invoked()
        {
            var request = new RunRequest($@"
using System;

public static class Hello
{{
    public static void Main()
    {{
        Console.WriteLine(""Hello there!"");
    }}
}}");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);
            result.ShouldSucceedWithOutput("Hello there!");
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

            result.ShouldSucceedWithOutput("Hello there!");
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

            result.ShouldSucceedWithOutput("Hello there!");
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

            result.ShouldSucceedWithOutput("Hello there!");
        }

        [Fact]
        public async Task Response_shows_warnings_with_successful_compilation()
        {
            var output = nameof(Response_shows_warnings_with_successful_compilation);

            var request = new RunRequest($@"
using System;
using System;
public static class Hello
{{
    public static void Main()
    {{
        Console.WriteLine(""{output}"");
    }}
}}");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.Diagnostics.Should().Contain(d => d.Severity == DiagnosticSeverity.Warning);

        }

        [Fact]
        public async Task Response_shows_warnings_when_compilation_fails()
        {
            var output = nameof(Response_shows_warnings_when_compilation_fails);

            var request = new RunRequest($@"
using System;
using System;
public static class Hello
{{
    public static void Main()
    {{
        Console.WriteLine(""{output}"")
    }}
}}");

            var server = GetWorkspaceServer();

            var result = await server.Run(request);

            result.Diagnostics.Should().Contain(d => d.Severity == DiagnosticSeverity.Warning);
        }

        [Fact]
        public async Task Get_diagnostics_produces_appropriate_diagnostics_for_display_to_user()
        {
            var request = new RunRequest(@"
using System;

public static class Hello
{
    public static void Main()
    {
        Console.WriteLine(""Hello there!"")
    }
}");

            var server = GetWorkspaceServer();

            var result = await server.GetDiagnostics(request);

            result.Diagnostics.Should().NotContain(d => d.Id == "CS7022"); // Not "ignoring main in script"
            result.Diagnostics.Should().Contain(d => d.Id == "CS1002"); // Yes missing semicolon
        }
    }
}
