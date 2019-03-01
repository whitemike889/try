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
        private const string CompilingProgramCs = @"
using System;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine();
    }
}";

        private const string CsprojContents = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.1</TargetFramework>
  </PropertyGroup>
</Project>
";



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
                new VerifyOptions(root),
                console,
                () => directoryAccessor,
                PackageRegistry.CreateForTryMode(root, null));

            console.Out
                   .ToString()
                   .Should()
                   .Match($"*{root}doc.md*Line 3:*{root}Program.cs (in project UNKNOWN)*File not found: ./Program.cs*No project file or package specified*");
        }

        [Fact]
        public async Task Files_are_listed()
        {
            var root = Create.EmptyWorkspace(isRebuildablePackage: true).Directory;

            var directoryAccessor = new InMemoryDirectoryAccessor(root, root)
                                    {
                                        ("some.csproj", CsprojContents),
                                        ("Program.cs", CompilingProgramCs),
                                        ("doc.md", @"
```cs Program.cs
```
")
                                    }.CreateFiles();

            var console = new TestConsole();

            await VerifyCommand.Do(
                new VerifyOptions(root),
                console,
                () => directoryAccessor,
                PackageRegistry.CreateForTryMode(root, null));

            _output.WriteLine(console.Out.ToString());

            CodeManipulation.EnforceLF(console.Out
                                     .ToString()
                                     .Trim())
                   .Should()
                   .Match(
                       CodeManipulation.EnforceLF($@"{root}{Path.DirectorySeparatorChar}doc.md*Line 2:*{root}{Path.DirectorySeparatorChar}Program.cs (in project {root}{Path.DirectorySeparatorChar}some.csproj)*"));
        }

        [Fact]
        public async Task When_there_are_no_markdown_errors_then_return_code_is_0()
        {
            var rootDirectory = Create.EmptyWorkspace(isRebuildablePackage: true).Directory;

            var directoryAccessor = new InMemoryDirectoryAccessor(rootDirectory, rootDirectory)
                                    {
                                        ("some.csproj", CsprojContents),
                                        ("Program.cs", CompilingProgramCs),
                                        ("doc.md", @"
```cs Program.cs
```
")
                                    }.CreateFiles();

            var console = new TestConsole();

            var resultCode = await VerifyCommand.Do(
                                 new VerifyOptions(rootDirectory),
                                 console,
                                 () => directoryAccessor,
                                 PackageRegistry.CreateForTryMode(rootDirectory, null));

            _output.WriteLine(console.Out.ToString());

            resultCode.Should().Be(0);
        }

        [Fact]
        public async Task When_there_are_markdown_errors_then_return_code_is_nonzero()
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
                                 new VerifyOptions(rootDirectory),
                                 console,
                                 () => directoryAccessor,
                                 PackageRegistry.CreateForTryMode(rootDirectory, null));

            resultCode.Should().NotBe(0);
        }

        
        [Fact]
        public async Task When_there_are_no_files_found_then_return_code_is_nonzero()
        {
            var rootDirectory = Create.EmptyWorkspace(isRebuildablePackage: true).Directory;

            var directoryAccessor = new InMemoryDirectoryAccessor(rootDirectory);          

            var console = new TestConsole();

            var resultCode = await VerifyCommand.Do(
                                 new VerifyOptions(rootDirectory),
                                 console,
                                 () => directoryAccessor,
                                 PackageRegistry.CreateForTryMode(rootDirectory, null));

            resultCode.Should().NotBe(0);
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
                                 new VerifyOptions(rootDirectory),
                                 console,
                                 () => directoryAccessor,
                                 PackageRegistry.CreateForTryMode(rootDirectory, null));

            console.Out.ToString().Should().Contain("Session cannot span projects or packages: --session one");

            resultCode.Should().NotBe(0);
        }

        [Theory]
        [InlineData("")]
        [InlineData("--region mask")]
        public async Task Verify_shows_diagnostics_for_compilation_failures(string args)
        {
            var directory = Create.EmptyWorkspace(isRebuildablePackage: true).Directory;

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
                                 new VerifyOptions(directory),
                                 console,
                                 () => directoryAccessor,
                                 PackageRegistry.CreateForTryMode(directory, null));

            _output.WriteLine(console.Out.ToString());

            console.Out.ToString().Should().Contain($"Build failed for project {directoryAccessor.GetFullyQualifiedPath(new RelativeFilePath("sample.csproj"))}");

            resultCode.Should().NotBe(0);
        }

        [Fact]
        public async Task When_there_are_compilation_errors_outside_the_mask_then_they_are_displayed()
        {
            var rootDirectory = Create.EmptyWorkspace(isRebuildablePackage: true).Directory;

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
                                         CsprojContents)
                                    }.CreateFiles();

            var console = new TestConsole();

            var resultCode = await VerifyCommand.Do(
                                 new VerifyOptions(rootDirectory),
                                 console,
                                 () => directoryAccessor,
                                 PackageRegistry.CreateForTryMode(rootDirectory, null));

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
                new VerifyOptions(root),
                console,
                () => directoryAccessor,
                PackageRegistry.CreateForTryMode(root, null));
            
            _output.WriteLine(console.Out.ToString());

            console.Out
                   .ToString()
                   .Should()
                   .NotContain("Compiling samples for session");
        }


        [Fact]
        public async Task If_a_new_file_is_added_and_verify_is_called_the_compile_errors_in_it_are_emitted()
        {
            var rootDirectory = Create.EmptyWorkspace(isRebuildablePackage:true).Directory;

            var directoryAccessor = new InMemoryDirectoryAccessor(rootDirectory, rootDirectory)
                                    {
                                        ("Program.cs", $@"
    using System;

    public class Program
    {{
        public static void Main(string[] args)
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
                                         CsprojContents)
                                    }.CreateFiles();

            var console = new TestConsole();

            PackageRegistry packageRegistry = PackageRegistry.CreateForTryMode(rootDirectory, null);
            var resultCode = await VerifyCommand.Do(
                                 new VerifyOptions(rootDirectory),
                                 console,
                                 () => directoryAccessor,
                                 packageRegistry);

            _output.WriteLine(console.Out.ToString());
            resultCode.Should().Be(0);

            File.WriteAllText(directoryAccessor.GetFullyQualifiedPath(new RelativeFilePath("Sample.cs")).FullName, "DOES NOT COMPILE");
            
            resultCode = await VerifyCommand.Do(
                                 new VerifyOptions(rootDirectory),
                                 console,
                                 () => directoryAccessor,
                                 packageRegistry);

            _output.WriteLine(console.Out.ToString());

            console.Out.ToString()
                   .Should().Contain("Build failed")
                   .And.Contain("Sample.cs(1,10): error CS1002: ; expected");

            resultCode.Should().NotBe(0);
        }

        [Fact]
        public async Task When_the_file_is_modified_and_errors_are_added_verify_command_shows_the_errors()
        {
            var rootDirectory = Create.EmptyWorkspace(isRebuildablePackage: true).Directory;

            string validCode = $@"
    using System;

    public class Program
    {{
        public static void Main(string[] args)
        {{
#region mask
            Console.WriteLine();
#endregion
        }}
    }}";

            string invalidCode = $@"
    using System;

    public class Program
    {{
        public static void Main(string[] args)                         DOES NOT COMPILE
        {{
#region mask
            Console.WriteLine();
#endregion
        }}
    }}";

            var directoryAccessor = new InMemoryDirectoryAccessor(rootDirectory, rootDirectory)
                                    {
                                        ("Program.cs", validCode),
                                        ("sample.md", $@"
```cs Program.cs --region mask
```"),
                                        ("sample.csproj",
                                         CsprojContents)
                                    }.CreateFiles();

            var console = new TestConsole();

            var packageRegistry = PackageRegistry.CreateForTryMode(rootDirectory, null);
            var resultCode = await VerifyCommand.Do(
                                 new VerifyOptions(rootDirectory),
                                 console,
                                 () => directoryAccessor,
                                 packageRegistry);

            resultCode.Should().Be(0);

            File.WriteAllText(directoryAccessor.GetFullyQualifiedPath(new RelativeFilePath("Program.cs")).FullName, invalidCode);

            resultCode = await VerifyCommand.Do(
                                 new VerifyOptions(rootDirectory),
                                 console,
                                 () => directoryAccessor,
                                 packageRegistry);

            _output.WriteLine(console.Out.ToString());

            console.Out.ToString()
                   .Should().Contain("Build failed")
                   .And.Contain("Program.cs(6,72): error CS1002: ; expected");

            resultCode.Should().NotBe(0);
        }
    }
}