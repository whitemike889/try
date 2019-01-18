using FluentAssertions;
using System.IO;
using Xunit;
using MLS.Project.Generators;

namespace MLS.Agent.Tests
{
    public class MarkdownProjectTests
    {
        [Fact]
        public void Should_return_list_of_all_markdown_files()
        {
            var rootDir = TestAssets.BasicConsole;
            var project = new MarkdownProject(new StartupOptions(rootDirectory: rootDir));
            var file1 = TestAssets.GetFileAtPath(rootDir, "Readme.md");
            var file2 = TestAssets.GetFileAtPath(rootDir, "Subdirectory", "Tutorial.md");

            var files = project.GetAllFiles();

            files.Should().HaveCount(2);
            files.Should().Contain(f => f.FileInfo.FullName.Equals(file1.FullName));
            files.Should().Contain(f => f.FileInfo.FullName.Equals(file2.FullName));
        }

        [Fact]
        public void Should_return_false_for_nonexistent_file()
        {
            var rootDir = TestAssets.BasicConsole;
            var project = new MarkdownProject(new StartupOptions(rootDirectory: rootDir));
            var path = "DOESNOTEXIST";

            project.TryGetHtmlContent(path, out string content).Should().BeFalse();
        }

        [Fact]
        public void Should_return_true_and_get_html_content_for_the_passed_path()
        {
            var rootDir = TestAssets.BasicConsole;
            var project = new MarkdownProject(new StartupOptions(rootDirectory: rootDir));
            var path = "Readme.md";

            project.TryGetHtmlContent(path, out string content).Should().BeTrue();
            content.Should().Contain("<em>markdown file</em>");  
        }

        [Fact]
        public void Should_return_true_and_get_html_content_for_subdirectory_paths()
        {
            var rootDir = TestAssets.BasicConsole;
            var project = new MarkdownProject(new StartupOptions(rootDirectory: rootDir));
            var path = Path.Combine("Subdirectory", "Tutorial.md");

            project.TryGetHtmlContent(path, out string content).Should().BeTrue();
            content.Should().Contain("<em>tutorial file</em>");
        }

        [Fact]
        public void Should_parse_the_file_and_replace_paths_inside_code_section_with_code_inside_trydotnet_config_pre_tag()
        {
            var rootDir = TestAssets.BasicConsole;
            var project = new MarkdownProject(new StartupOptions(rootDirectory: rootDir));
            var path = "Readme.md";
            var expectedValue =
@"<pre style=""border: none"" height=""300px"" width=""800px"" data-trydotnet-mode=""editor"" data-trydotnet-project-template=""console"" data-trydotnet-session-id=""a"" height=""300px"" width=""800px"">
using System;

namespace BasicConsoleApp
{
    class Program
    {
        static void MyProgram(string[] args)
        {
            Console.WriteLine(&quot;Hello World!&quot;);
        }
    }
}

</pre>".EnforceLF();

            project.TryGetHtmlContent(path, out string content).Should().BeTrue();
            content.EnforceLF().Should().Contain(expectedValue);
        }
    }
}
