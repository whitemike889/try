using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using MLS.Agent.CommandLine;
using WorkspaceServer;
using WorkspaceServer.Tests;
using Xunit;
using Xunit.Abstractions;
using CodeManipulation = MLS.Project.Generators.CodeManipulation;

namespace MLS.Agent.Tests
{
    public class VerifyCommandTests
    {
        private readonly ITestOutputHelper _output;

        public VerifyCommandTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Errors_are_written_to_std_out()
        {
            var root = new DirectoryInfo(Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()));

            var directoryAccessor = new InMemoryDirectoryAccessor(root, root)
                                    {
                                        ("doc.md", @"
This is some sample code:
```cs Program.cs
```
")
                                    };

            var console = new TestConsole();

            await VerifyCommand.Do(
                new VerifyOptions(root, false),
                console,
                () => directoryAccessor,
                new PackageRegistry());

            console.Out
                   .ToString()
                   .Should()
                   .Match($"*{root}doc.md*Line 3:*{root}Program.cs (in project UNKNOWN)*File not found: ./Program.cs*No project file or package specified*");
        }

        [Fact]
        public async Task Files_are_listed()
        {
            var root = new DirectoryInfo(Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()));

            var directoryAccessor = new InMemoryDirectoryAccessor(root, root)
                                    {
                                        ("some.csproj", ""),
                                        ("Program.cs", ""),
                                        ("doc.md", @"
```cs Program.cs
```
")
                                    };

            var console = new TestConsole();

            await VerifyCommand.Do(
                new VerifyOptions(root, false),
                console,
                () => directoryAccessor,
                new PackageRegistry());

            _output.WriteLine(console.Out.ToString());

            CodeManipulation.EnforceLF(console.Out
                                     .ToString()
                                     .Trim())
                   .Should()
                   .Match(
                       CodeManipulation.EnforceLF($@"{root}doc.md*Line 2:*{root}Program.cs (in project {root}some.csproj)"));
        }

        [Fact]
        public async Task When_there_are_no_markdown_errors_then_return_code_is_0()
        {
            var rootDirectory = new DirectoryInfo(".");

            var directoryAccessor = new InMemoryDirectoryAccessor(rootDirectory)
                                    {
                                        ("some.csproj", ""),
                                        ("Program.cs", ""),
                                        ("doc.md", @"
```cs Program.cs
```
")
                                    };

            var console = new TestConsole();

            var resultCode = await VerifyCommand.Do(
                                 new VerifyOptions(rootDirectory, false),
                                 console,
                                 () => directoryAccessor,
                                 new PackageRegistry());

            _output.WriteLine(console.Out.ToString());

            resultCode.Should().Be(0);
        }

        [Fact]
        public async Task When_there_are_markdown_errors_then_return_code_is_1()
        {
            var rootDirectory = new DirectoryInfo(".");

            var directoryAccessor = new InMemoryDirectoryAccessor(rootDirectory)
                                    {
                                        ("doc.md", @"
```cs Program.cs
```
")
                                    };

            var console = new TestConsole();

            var resultCode = await VerifyCommand.Do(
                                 new VerifyOptions(rootDirectory, false),
                                 console,
                                 () => directoryAccessor,
                                 new PackageRegistry());

            resultCode.Should().Be(1);
        }

        [Theory]
        [InlineData(@"
```cs Program.cs --session one --project a.csproj
```
```cs Program.cs --session one --project b.csproj
```")]
        [InlineData(@"
```cs Program.cs --session one --package some-package
```
```cs Program.cs --session one --project b.csproj
```")]
        public async Task Returns_an_error_when_a_session_has_more_than_one_package_or_project(string mdFileContents)
        {
            var rootDirectory = new DirectoryInfo(".");

            var directoryAccessor = new InMemoryDirectoryAccessor(rootDirectory)
                                    {
                                        ("doc.md", mdFileContents),
                                        ("a.csproj", ""),
                                        ("b.csproj", ""),
                                    };

            var console = new TestConsole();

            var resultCode = await VerifyCommand.Do(
                                 new VerifyOptions(rootDirectory, false),
                                 console,
                                 () => directoryAccessor,
                                 new PackageRegistry());

            console.Out.ToString().Should().Contain("Session cannot span projects or packages: --session one");

            resultCode.Should().NotBe(0);
        }

        [Theory]
        [InlineData("")]
        [InlineData("--region mask")]
        public async Task Verify_shows_diagnostics_for_complation_failures(string args)
        {
            var directory = Create.EmptyWorkspace().Directory;

            var directoryAccessor = new InMemoryDirectoryAccessor(directory, directory)
                                    {
                                        ("Program.cs", $@"
    public class Program
    {{
        public static void Main(string[] args)
        {{
#region mask
            Console.WriteLine()
#endregion
        }}
    }}"),
                                        ("sample.md", $@"
```cs {args} Program.cs
```"),
                                        ("sample.csproj",
                                         @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.1</TargetFramework>
  </PropertyGroup>
</Project>
")
                                    }.CreateFiles();

            var console = new TestConsole();

            var resultCode = await VerifyCommand.Do(
                                 new VerifyOptions(directory, true),
                                 console,
                                 () => directoryAccessor,
                                 new PackageRegistry());

            _output.WriteLine(console.Out.ToString());

            console.Out.ToString().Should().Contain($"Build failed for project {directoryAccessor.GetFullyQualifiedPath(new RelativeFilePath("sample.csproj"))}");

            resultCode.Should().NotBe(0);
        }

        [Fact]
        public async Task When_there_are_compilation_errors_outside_the_mask_then_they_are_displayed()
        {
            var rootDirectory = Create.EmptyWorkspace().Directory;

            var directoryAccessor = new InMemoryDirectoryAccessor(rootDirectory, rootDirectory)
                                    {
                                        ("Program.cs", $@"
    using System;

    public class Program
    {{
        public static void Main(string[] args)                         DOES NOT COMPILE
        {{
#region mask
            Console.WriteLine();
#endregion
        }}
    }}"),
                                        ("sample.md", $@"
```cs Program.cs --region mask
```"),
                                        ("sample.csproj",
                                         @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.1</TargetFramework>
  </PropertyGroup>
</Project>
")
                                    }.CreateFiles();

            var console = new TestConsole();

            var resultCode = await VerifyCommand.Do(
                                 new VerifyOptions(rootDirectory, true),
                                 console,
                                 () => directoryAccessor,
                                 new PackageRegistry());

            _output.WriteLine(console.Out.ToString());

            console.Out.ToString()
                   .Should().Contain("Build failed")
                   .And.Contain("Program.cs(6,72): error CS1002: ; expected");

            resultCode.Should().NotBe(0);
        }
        
        [Fact]
        public async Task When_there_are_code_fence_options_errors_then_compilation_is_not_attempted()
        {
            var root = new DirectoryInfo(Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()));

            var directoryAccessor = new InMemoryDirectoryAccessor(root, root)
                                    {
                                        ("doc.md", @"
This is some sample code:
```cs Program.cs
```
")
                                    };

            var console = new TestConsole();

            await VerifyCommand.Do(
                new VerifyOptions(root, true),
                console,
                () => directoryAccessor,
                new PackageRegistry());
            
            _output.WriteLine(console.Out.ToString());

            console.Out
                   .ToString()
                   .Should()
                   .NotContain("Compiling samples for session");
        }
    }
}