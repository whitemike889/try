using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace LanguageServer.Tests
{
    public class DotDotnetLanguageServerTests
    {
        [Fact]
        public async Task Response_indicates_when_compile_is_successful_and_signature_is_correct()
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

            var server = new DotDotnetLanguageServer();

            var result = await server.CompileAndExecute(request);

            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task Response_shows_program_output_when_compile_is_successful_and_signature_is_correct()
        {
            var request = new CompileAndExecuteRequest(@"
using System;

public static class Hello 
{
    public static void Main() 
    { 
        Console.WriteLine(""Hello there!"");
    } 
}
");

            var server = new DotDotnetLanguageServer();

            var result = await server.CompileAndExecute(request);

            result.Output.Should().Contain("Hello there!");
        }

        [Fact]
        public async Task Response_indicates_when_compile_is_unsuccessful()
        {
            var request = new CompileAndExecuteRequest(@"
using NonexistentNamespace;");

            var server = new DotDotnetLanguageServer();

            var result = await server.CompileAndExecute(request);

            result.Succeeded.Should().BeFalse();

            result.Output.Should()
                  .Contain("(2,7): error CS0246: The type or namespace name 'NonexistentNamespace' could not be found (are you missing a using directive or an assembly reference?)");
        }
    }
}
