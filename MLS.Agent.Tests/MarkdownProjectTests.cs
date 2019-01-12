using FluentAssertions;
using System.IO;
using Xunit;
using MLS.Project.Generators;
using HtmlAgilityPack;

namespace MLS.Agent.Tests
{
    public class MarkdownProjectTests
    {
        [Fact]
        public void Should_return_list_of_all_markdown_files()
        {
            var workingDir = TestAssets.SampleConsole;
            var dirAccessor = new InMemoryDirectoryAccessor(workingDir)
            {
                ("Readme.md", ""),
                ("Subdirectory/Tutorial.md", ""),
                ("Program.cs", "")
            };

            var project = new MarkdownProject(dirAccessor);
            var file1 = TestAssets.GetFileAtPath(workingDir, "Readme.md");
            var file2 = TestAssets.GetFileAtPath(workingDir, "Subdirectory", "Tutorial.md");

            var files = project.GetAllMarkdownFiles();

            files.Should().HaveCount(2);
            files.Should().Contain(f => f.FileInfo.FullName.Equals(file1.FullName));
            files.Should().Contain(f => f.FileInfo.FullName.Equals(file2.FullName));
        }

        [Fact]
        public void Should_return_false_for_nonexistent_file()
        {
            var workingDir = TestAssets.SampleConsole;
            var dirAccessor = new InMemoryDirectoryAccessor(workingDir);
            var project = new MarkdownProject(dirAccessor);
            var path = "DOESNOTEXIST";

            project.TryGetHtmlContent(path, out string content).Should().BeFalse();
        }

        [Fact]
        public void Should_return_true_and_get_html_content_for_the_passed_path()
        {
            var workingDir = TestAssets.SampleConsole;
            var dirAccessor = new InMemoryDirectoryAccessor(workingDir)
            {
                ("Readme.md", "This is a sample *markdown file*")
            };

            var project = new MarkdownProject(dirAccessor);
            var path = "Readme.md";

            project.TryGetHtmlContent(path, out string content).Should().BeTrue();
            content.Should().Contain("<em>markdown file</em>");
        }

        [Fact]
        public void Should_return_true_and_get_html_content_for_subdirectory_paths()
        {
            var workingDir = TestAssets.SampleConsole;
            var dirAccessor = new InMemoryDirectoryAccessor(workingDir)
            {
                ("Subdirectory/Tutorial.md", "This is a sample *tutorial file*")
            };
            var project = new MarkdownProject(dirAccessor);
            var path = Path.Combine("Subdirectory", "Tutorial.md");

            project.TryGetHtmlContent(path, out string content).Should().BeTrue();
            content.Should().Contain("<em>tutorial file</em>");
        }

        [Fact]
        public void Should_parse_the_file_and_insert_with_code_present_in_file()
        {
            var workingDir = TestAssets.SampleConsole;
            var codeContent = @"using System;

namespace BasicConsoleApp
{
    class Program
    {
        static void MyProgram(string[] args)
        {
            Console.WriteLine(""Hello World!"");
        }
    }
}".EnforceLF();
            var dirAccessor = new InMemoryDirectoryAccessor(workingDir)
            {
                ("Program.cs", codeContent),
                ("Readme.md",
@"This is a sample *markdown file*

```cs Program.cs
```"),
                ("sample.csproj", ""),
            };

            var project = new MarkdownProject(dirAccessor);
            project.TryGetHtmlContent("Readme.md", out string content).Should().BeTrue();
            content.EnforceLF().Should().Contain(codeContent.HtmlEncode());
        }
    }
}
