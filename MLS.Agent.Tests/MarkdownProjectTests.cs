using System.IO;
using System.Linq;
using FluentAssertions;
using HtmlAgilityPack;
using MLS.Project.Generators;
using Xunit;

namespace MLS.Agent.Tests
{
    public class MarkdownProjectTests
    {
        public class GetAllMarkdownFiles
        {
            [Fact]
            public void Returns_list_of_all_relative_paths_to_all_markdown_files()
            {
                var workingDir = TestAssets.SampleConsole;
                var dirAccessor = new InMemoryDirectoryAccessor(workingDir)
                                  {
                                      ("Readme.md", ""),
                                      ("Subdirectory/Tutorial.md", ""),
                                      ("Program.cs", "")
                                  };

                var project = new MarkdownProject(dirAccessor);

                var files = project.GetAllMarkdownFiles();

                files.Should().HaveCount(2);
                files.Should().Contain(f => f.Path.Value.Equals("./Readme.md"));
                files.Should().Contain(f => f.Path.Value.Equals("Subdirectory/Tutorial.md"));
            }
        }

        public class TryGetHtmlContent
        {
            [Fact]
            public void Returns_false_for_nonexistent_file()
            {
                var workingDir = TestAssets.SampleConsole;
                var dirAccessor = new InMemoryDirectoryAccessor(workingDir);
                var project = new MarkdownProject(dirAccessor);
                var path = new RelativeFilePath("DOESNOTEXIST");

                project.TryGetHtmlContent(path, out _).Should().BeFalse();
            }

            [Fact]
            public void Returns_true_and_get_html_content_for_the_passed_path()
            {
                var workingDir = TestAssets.SampleConsole;
                var dirAccessor = new InMemoryDirectoryAccessor(workingDir)
                                  {
                                      ("Readme.md", "This is a sample *markdown file*")
                                  };

                var project = new MarkdownProject(dirAccessor);
                var path = new RelativeFilePath("Readme.md");

                project.TryGetHtmlContent(path, out var content).Should().BeTrue();
                content.Should().Contain("<em>markdown file</em>");
            }

            [Fact]
            public void Returns_true_and_get_html_content_for_subdirectory_paths()
            {
                var workingDir = TestAssets.SampleConsole;
                var dirAccessor = new InMemoryDirectoryAccessor(workingDir)
                                  {
                                      ("Subdirectory/Tutorial.md", "This is a sample *tutorial file*")
                                  };
                var project = new MarkdownProject(dirAccessor);
                var path = new RelativeFilePath(Path.Combine("Subdirectory", "Tutorial.md"));

                project.TryGetHtmlContent(path, out var content).Should().BeTrue();
                content.Should().Contain("<em>tutorial file</em>");
            }

            [Fact]
            public void When_file_argument_is_specified_then_it_inserts_code_present_in_csharp_file()
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
                                      ("sample.csproj", "")
                                  };

                var project = new MarkdownProject(dirAccessor);
                project.TryGetHtmlContent(new RelativeFilePath("Readme.md"), out var content).Should().BeTrue();
                content.EnforceLF().Should().Contain(codeContent.HtmlEncode());
            }

            [Fact]
            public void When_no_source_file_argument_is_specified_then_it_does_not_replace_fenced_csharp_code()
            {
                var fencedCode = @"// this is the actual embedded code";
                var dirAccessor = new InMemoryDirectoryAccessor(TestAssets.SampleConsole)
                                  {
                                      ("Readme.md",
                                       $@"This is a sample *markdown file*

```cs
{fencedCode}
```")
                                  };

                var project = new MarkdownProject(dirAccessor);
                project.TryGetHtmlContent(new RelativeFilePath("Readme.md"), out var content).Should().BeTrue();

                content.EnforceLF().Should().Contain(fencedCode.HtmlEncode());
            }

            [Fact]
            public void Should_parse_markdown_file_and_insert_code_from_paths_relative_to_the_markdown_file()
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

                var package = "../src/sample/sample.csproj";

                var dirAccessor = new InMemoryDirectoryAccessor(workingDir)
                                  {
                                      ("src/sample/Program.cs", codeContent),
                                      ("src/sample/sample.csproj", ""),
                                      ("docs/Readme.md",
                                       $@"
```cs --project {package} ../src/sample/Program.cs
```")
                                  };

                var project = new MarkdownProject(dirAccessor);
                project.TryGetHtmlContent(new RelativeFilePath("docs/Readme.md"), out var content).Should().BeTrue();
                content.EnforceLF().Should().Contain(codeContent.HtmlEncode());
            }

            [Fact]
            public void Should_parse_markdown_file_and_set_package_with_fully_resolved_path()
            {
                var workingDir = TestAssets.SampleConsole;
                var packagePathRelativeToBaseDir = "src/sample/sample.csproj";

                var dirAccessor = new InMemoryDirectoryAccessor(workingDir)
                                  {
                                      ("src/sample/Program.cs", ""),
                                      (packagePathRelativeToBaseDir, ""),
                                      ("docs/Readme.md",
                                       $@"```cs --project ../{packagePathRelativeToBaseDir} ../src/sample/Program.cs
```")
                                  };

                var project = new MarkdownProject(dirAccessor);
                project.TryGetHtmlContent(new RelativeFilePath("docs/Readme.md"), out var content).Should().BeTrue();
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(content);
                var output = htmlDocument.DocumentNode
                                         .SelectSingleNode("//pre/code").Attributes["data-trydotnet-package"];

                var fullProjectPath = dirAccessor.GetFullyQualifiedPath(new RelativeFilePath(packagePathRelativeToBaseDir));
                output.Value.Should().Be(fullProjectPath.FullName);
            }
        }

        public class GetAllProjects
        {
            [Fact]
            public void Returns_all_projects_referenced_from_all_markdown_files()
            {
                var project = new MarkdownProject(
                    new InMemoryDirectoryAccessor(new DirectoryInfo(Directory.GetCurrentDirectory()))
                    {
                        ("readme.md", @"
```cs --project ../Project1/Console1.csproj
```
```cs --project ../Project2/Console2.csproj
```
                        "),
                        ("../Project1/Console1.csproj", @""),
                        ("../Project2/Console2.csproj", @"")
                    });

                project.GetAllMarkdownFiles()
                       .SelectMany(f => f.GetCodeLinkBlocks().Select(b => b.ProjectFile))
                       .Should()
                       .Contain(p => p.Directory.Name == "Project1")
                       .And
                       .Contain(p => p.Directory.Name == "Project2");
            }
        }
    }
}