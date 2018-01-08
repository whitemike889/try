using System;
using FluentAssertions;
using System.Linq;
using Recipes;
using WorkspaceServer.Models.Execution;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class RunRequestTests
    {
        [Fact]
        public void RunRequest_from_Orchestrator_can_be_deserialized()
        {
            var requestJson = @"
{
    ""Source"": ""Console.WriteLine(5);"", 
    ""Usings"": [
        ""using Xunit;""
    ]
}";

            var runRequest = requestJson.FromJsonTo<RunRequest>();

            runRequest.Sources.Should().HaveCount(2);
            runRequest.Sources.ElementAt(1).Should().Contain(@"Console.WriteLine(5);");
            runRequest.Usings.Should().Contain("using Xunit;");
        }

        [Fact]
        public void When_code_is_fragment_then_GetSourceFiles_returns_two_files()
        {
            var code = @"Console.WriteLine(""hello"");";

            var request = new RunRequest(code);

            var sourceFiles = request.GetSourceFiles();

            sourceFiles.Should().HaveCount(2);
            sourceFiles.ElementAt(0).Name.Should().Be("Program.cs");
            sourceFiles.ElementAt(1).Name.Should().Be("2.cs");
        }
    }
}
