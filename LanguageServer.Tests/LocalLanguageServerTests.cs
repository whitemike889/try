using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace LanguageServer.Tests
{
    public class LocalLanguageServerTests : LanguageServerTests
    {
        protected override ILanguageServer GetLanguageServer()
        {
            var directoryName = DateTime.Now.ToString("yyyy.MM.dd-hh.mm.ss");

            var directory = new DirectoryInfo(directoryName);

            return new LocalLanguageServer(directory);
        }

        public LocalLanguageServerTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task Code_can_be_updated_and_rerun_in_the_same_workspace()
        {
            var server = GetLanguageServer();

            ProcessResult result;

            result = await server.CompileAndExecute(
                         new BuildAndRunRequest(@"Console.WriteLine(i don't compile!);"));

            result.Output.Should().Contain(line => line.Contains("Syntax error"));

            result = await server.CompileAndExecute(
                         new BuildAndRunRequest(@"
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
