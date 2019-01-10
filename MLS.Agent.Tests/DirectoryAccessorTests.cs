using FluentAssertions;
using MLS.Agent.Markdown;
using System;
using System.IO;
using Xunit;

namespace MLS.Agent.Tests
{
    public abstract class DirectoryAccessorTests
    {
        public abstract IDirectoryAccessor GetDirectory(DirectoryInfo dirInfo);

        [Fact]
        public void When_the_file_exists_FileExists_returns_true()
        {
            var testDir = TestAssets.SampleConsole;
            GetDirectory(testDir).FileExists("Program.cs").Should().BeTrue();
        }

        [Fact]
        public void When_the_filepath_is_empty_FileExists_returns_false()
        {
            var testDir = TestAssets.SampleConsole;
            GetDirectory(testDir).FileExists("").Should().BeFalse();
        }

        [Fact]
        public void When_the_filepath_is_null_FileExists_returns_false()
        {
            var testDir = TestAssets.SampleConsole;
            GetDirectory(testDir).Invoking(d => d.FileExists(null)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void When_the_filepath_contains_invalid_path_characters_FileExists_returns_false()
        {
            var testDir = TestAssets.SampleConsole;
            var characters = Path.GetInvalidPathChars();
            GetDirectory(testDir).Invoking(d => d.FileExists($"{characters[0]}")).Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(@"Subdirectory/AnotherProgram.cs")]
        [InlineData(@"Subdirectory\AnotherProgram.cs")]
        public void When_the_filepath_contains_subdirectory_paths_FileExists_returns_true(string filepath)
        {
            var testDir = TestAssets.SampleConsole;
            GetDirectory(testDir).FileExists(filepath).Should().BeTrue();
        }

        [Theory]
        [InlineData(@"../Program.cs")]
        [InlineData(@"..\Program.cs")]
        public void When_the_filepath_contains_a_path_that_looks_upward_in_tree_then_FileExists_returns_the_text(string filePath)
        {
            var testDir = new DirectoryInfo(Path.Combine(TestAssets.SampleConsole.FullName, "Subdirectory"));
            GetDirectory(testDir).FileExists(filePath).Should().BeTrue();
        }

        [Fact]
        public void When_the_filepath_contains_an_existing_file_ReadAllText_returns_the_text()
        {
            var testDir = TestAssets.SampleConsole;
            GetDirectory(testDir).ReadAllText("Program.cs").Should().Contain("Hello World!");
        }

        [Theory]
        [InlineData(@"Subdirectory/AnotherProgram.cs")]
        [InlineData(@"Subdirectory\AnotherProgram.cs")]
        public void When_the_filepath_contains_an_existing_file_from_subdirectory_then_ReadAllText_returns_the_text(string filePath)
        {
            var testDir = TestAssets.SampleConsole;
            GetDirectory(testDir).ReadAllText(filePath).Should().Contain("Hello from Another Program!");
        }

        [Theory]
        [InlineData(@"../Program.cs")]
        [InlineData(@"..\Program.cs")]
        public void When_the_filepath_contains_a_path_that_looks_upward_in_tree_then_ReadAllText_returns_the_text(string filePath)
        {
            var testDir = new DirectoryInfo(Path.Combine(TestAssets.SampleConsole.FullName, "Subdirectory"));
            GetDirectory(testDir).ReadAllText(filePath).Should().Contain("Hello World!");
        }
    }

    public class FileSystemDirectoryAccessorTests : DirectoryAccessorTests
    {
        public override IDirectoryAccessor GetDirectory(DirectoryInfo directoryInfo)
        {
            return new FileSystemDirectoryAccessor(directoryInfo);
        }
    }

    public class InMemoryDirectoryAccessorTests : DirectoryAccessorTests
    {
        public override IDirectoryAccessor GetDirectory(DirectoryInfo rootDirectory)
        {
            return new InMemoryDirectoryAccessor(rootDirectory)
            {
               ("BasicConsoleApp.csproj",
@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.1</TargetFramework>
  </PropertyGroup>

</Project>
"),
           ("Program.cs",
@"using System;

namespace BasicConsoleApp
{
    class Program
    {
        static void MyProgram(string[] args)
        {
            Console.WriteLine(""Hello World!"");
        }
    }
}"),
           ("Readme.md",
@"This is a sample *markdown file*

```cs Program.cs
```"),
            ("Subdirectory/Tutorial.md", "This is a sample *tutorial file*"),
            ("Subdirectory/AnotherProgram.cs",
@"using System;
using System.Collections.Generic;
using System.Text;

namespace MLS.Agent.Tests.TestProjects.BasicConsoleApp.Subdirectory
{
    class AnotherPorgram
    {
        static void MyAnotherProgram(string[] args)
        {
            Console.WriteLine(""Hello from Another Program!"");
        }
    }
    }
")
            };
        }
    }
}
   

