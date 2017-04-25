using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace LanguageServer.Tests
{
    public abstract class LanguageServerTests
    {
        protected abstract ILanguageServer GetLanguageServer();

        protected readonly ITestOutputHelper _output;

        protected LanguageServerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Response_indicates_when_compile_is_successful_and_signature_is_like_a_console_app()
        {
            var request = new CompileAndExecuteRequest(@"
using System;

public static class Hello 
{
    public static void Main() 
    {
    } 
}
");

            var server = GetLanguageServer();

            var result = await server.CompileAndExecute(request);

            _output.WriteLine(string.Join("\n", result.Output));

            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task Response_shows_program_output_when_compile_is_successful_and_signature_is_like_a_console_app()
        {
            var request = new CompileAndExecuteRequest(@"
using System;

public static class Hello 
{
    public static void Main() 
    { 
        Console.WriteLine(""Hello there!"");
    } 
}");

            var server = GetLanguageServer();

            var result = await server.CompileAndExecute(request);

            result.Output.Should().Contain("Hello there!");
        }

        [Fact]
        public async Task Response_shows_program_output_when_compile_is_successful_and_signature_is_a_fragment_containing_console_output()
        {
            var request = new CompileAndExecuteRequest(@"
var person = new { Name = ""Jeff"", Age = 20 };
var s = $""{person.Name} is {person.Age} year(s) old"";
Console.WriteLine(s);");

            var server = GetLanguageServer();

            var result = await server.CompileAndExecute(request);

            result.Output.Should().Contain("Jeff is 20 year(s) old");
        }

        [Fact]
        public async Task Response_indicates_when_compile_is_unsuccessful()
        {
            var request = new CompileAndExecuteRequest(@"
Console.WriteLine(banana);");

            var server = GetLanguageServer();

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