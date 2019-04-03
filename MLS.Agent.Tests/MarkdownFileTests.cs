using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HtmlAgilityPack;
using Microsoft.DotNet.Try.Markdown;
using MLS.Agent.CommandLine;
using MLS.Agent.Markdown;
using MLS.Agent.Tests.TestUtility;
using MLS.Project.Generators;
using WorkspaceServer;
using WorkspaceServer.PackageDiscovery;
using Xunit;

namespace MLS.Agent.Tests
{
    public class MarkdownFileTests
    {
        public class ToHtmlContent
        {
            [Fact]
            public async Task Returns_true_and_get_html_content_for_the_passed_path()
            {
                var workingDir = TestAssets.SampleConsole;
                var dirAccessor = new InMemoryDirectoryAccessor(workingDir)
                                  {
                                      ("Readme.md", "This is a sample *markdown file*")
                                  };

                var project = new MarkdownProject(dirAccessor, PackageRegistry.CreateForHostedMode());
                var path = new RelativeFilePath("Readme.md");

                project.TryGetMarkdownFile(path, out var markdownFile).Should().BeTrue();
                (await markdownFile.ToHtmlContentAsync()).ToString().Should().Contain("<em>markdown file</em>");
            }

            [Fact]
            public async Task Returns_true_and_get_html_content_for_subdirectory_paths()
            {
                var workingDir = TestAssets.SampleConsole;
                var dirAccessor = new InMemoryDirectoryAccessor(workingDir)
                                  {
                                      ("Subdirectory/Tutorial.md", "This is a sample *tutorial file*")
                                  };
                var project = new MarkdownProject(dirAccessor, PackageRegistry.CreateForHostedMode());
                var path = new RelativeFilePath(Path.Combine("Subdirectory", "Tutorial.md"));

                project.TryGetMarkdownFile(path, out var markdownFile).Should().BeTrue();
                (await markdownFile.ToHtmlContentAsync()).ToString().Should().Contain("<em>tutorial file</em>");
            }

            [Fact]
            public async Task When_file_argument_is_specified_then_it_inserts_code_present_in_csharp_file()
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

```cs --source-file Program.cs
```"),
                                      ("sample.csproj", "")
                                  };

                var project = new MarkdownProject(dirAccessor, PackageRegistry.CreateForHostedMode());
                project.TryGetMarkdownFile(new RelativeFilePath("Readme.md"), out var markdownFile).Should().BeTrue();
                (await markdownFile.ToHtmlContentAsync()).ToString().EnforceLF().Should().Contain(codeContent.HtmlEncode().ToString());
            }

            [Fact]
            public async Task When_no_source_file_argument_is_specified_then_it_does_not_replace_fenced_csharp_code()
            {
                var fencedCode = @"// this is the actual embedded code";
                var dirAccessor = new InMemoryDirectoryAccessor(TestAssets.SampleConsole)
                                  {
                                      ("Readme.md",
                                       $@"This is a sample *markdown file*

```cs
{fencedCode}
```"),
                                      ("sample.csproj", "")
                                  };

                var project = new MarkdownProject(dirAccessor, PackageRegistry.CreateForHostedMode());
                project.TryGetMarkdownFile(new RelativeFilePath("Readme.md"), out var markdownFile).Should().BeTrue();
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml((await markdownFile.ToHtmlContentAsync()).ToString());
                var output = htmlDocument.DocumentNode
                                         .SelectSingleNode("//pre/code").InnerHtml.EnforceLF();
                output.Should().Be($"{fencedCode}\n");
            }

            [Fact]
            public async Task Should_parse_markdown_file_and_insert_code_from_paths_relative_to_the_markdown_file()
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
```cs --project {package} --source-file ../src/sample/Program.cs
```")
                                  };

                var project = new MarkdownProject(dirAccessor, PackageRegistry.CreateForHostedMode());
                project.TryGetMarkdownFile(new RelativeFilePath("docs/Readme.md"), out var markdownFile).Should().BeTrue();
                (await markdownFile.ToHtmlContentAsync()).ToString().EnforceLF().Should().Contain(codeContent.HtmlEncode().ToString());
            }

            [Fact]
            public async Task Should_parse_markdown_file_and_set_package_with_fully_resolved_path()
            {
                var workingDir = TestAssets.SampleConsole;
                var packagePathRelativeToBaseDir = "src/sample/sample.csproj";

                var dirAccessor = new InMemoryDirectoryAccessor(workingDir)
                                  {
                                      ("src/sample/Program.cs", ""),
                                      (packagePathRelativeToBaseDir, ""),
                                      ("docs/Readme.md",
                                       $@"```cs --project ../{packagePathRelativeToBaseDir} --source-file ../src/sample/Program.cs
```")
                                  };

                var project = new MarkdownProject(dirAccessor, PackageRegistry.CreateForHostedMode());
                project.TryGetMarkdownFile(new RelativeFilePath("docs/Readme.md"), out var markdownFile).Should().BeTrue();
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml((await markdownFile.ToHtmlContentAsync()).ToString());
                var output = htmlDocument.DocumentNode
                                         .SelectSingleNode("//pre/code").Attributes["data-trydotnet-package"];

                var fullProjectPath = dirAccessor.GetFullyQualifiedPath(new RelativeFilePath(packagePathRelativeToBaseDir));
                output.Value.Should().Be(fullProjectPath.FullName);
            }

            [Fact]
            public async Task Should_include_the_code_from_source_file_and_not_the_fenced_code()
            {
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

                var workingDir = TestAssets.SampleConsole;
                var dirAccessor = new InMemoryDirectoryAccessor(workingDir)
                {
                    ("sample.csproj", ""),
                    ("Program.cs", codeContent),
                    ("Readme.md",
@"```cs --source-file Program.cs
Console.WriteLine(""This code should not appear"");
```"),
                };

                var project = new MarkdownProject(dirAccessor, PackageRegistry.CreateForHostedMode());
                project.TryGetMarkdownFile(new RelativeFilePath("Readme.md"), out var markdownFile).Should().BeTrue();
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml((await markdownFile.ToHtmlContentAsync()).ToString());
                var output = htmlDocument.DocumentNode
                                         .SelectSingleNode("//pre/code").InnerHtml.EnforceLF();

                output.Should().Contain($"{codeContent.HtmlEncode()}");
            }

            [Fact]
            public async Task Multiple_fenced_code_blocks_are_correctly_rendered()
            {
                var region1Code = @"Console.WriteLine(""I am region one code"");";
                var region2Code = @"Console.WriteLine(""I am region two code"");";
                var directory = TestAssets.SampleConsole;
                var codeContent = $@"using System;

namespace BasicConsoleApp
{{
    class Program
    {{
        static void MyProgram(string[] args)
        {{
            #region region1
            {region1Code}
            #endregion
            
            #region region2
            {region2Code}
            #endregion
        }}
    }}
}}".EnforceLF();

                var dirAccessor = new InMemoryDirectoryAccessor(directory)
                {
                    ("sample.csproj", ""),
                    ("Program.cs", codeContent),
                    ("Readme.md",
@"This is a markdown file with two regions
This is region 1
```cs --source-file Program.cs --region region1
//This part should not be included
```
This is region 2
```cs --source-file Program.cs --region region2
//This part should not be included as well
```
This is the end of the file")
                };

                var project = new MarkdownProject(dirAccessor, PackageRegistry.CreateForHostedMode());
                project.TryGetMarkdownFile(new RelativeFilePath("Readme.md"), out var markdownFile).Should().BeTrue();
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml((await markdownFile.ToHtmlContentAsync()).ToString());
                var codeNodes = htmlDocument.DocumentNode.SelectNodes("//pre/code");

                codeNodes.Should().HaveCount(2);
                codeNodes[0].InnerHtml.Should().Contain($"{region1Code.HtmlEncode()}");
                codeNodes[1].InnerHtml.Should().Contain($"{region2Code.HtmlEncode()}");
            }

            [Fact]
            public async Task Non_editable_code_inserts_code_present_in_markdown()
            {
                var console = new TestConsole();
                var asset = await LocalToolHelpers.CreateTool(console);
                var registry = new PackageRegistry(false, new LocalToolPackageDiscoveryStrategy(asset, asset));

                var project = new MarkdownProject(
                    new InMemoryDirectoryAccessor(new DirectoryInfo(Directory.GetCurrentDirectory()))
                    {
                        ("readme.md", @"
```cs --editable false
//some code to include
```
                        ")
                    },
                    registry);

                var thing = project.GetAllMarkdownFiles().Single();
                var text = (await thing.ToHtmlContentAsync()).ToString();
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(text);
                var codeNodes = htmlDocument.DocumentNode.SelectNodes("//pre/code[@data-trydotnet-mode='include']");
                codeNodes.Should().HaveCount(1);

                codeNodes.Single().InnerText.Should().Match(@"*//some code to include*");
            }

            [Fact]
            public async Task When_file_argument_is_specified_then_it_inserts_code_present_in_csharp_file_from_a_package()
            {
                var console = new TestConsole();
                var asset = await LocalToolHelpers.CreateTool(console);
                var registry = new PackageRegistry(false, new LocalToolPackageDiscoveryStrategy(asset, asset));

                var project = new MarkdownProject(
                    new InMemoryDirectoryAccessor(new DirectoryInfo(Directory.GetCurrentDirectory()))
                    {
                        ("readme.md", @"
```cs --source-file Program.cs --package console
```
                        ")
                    },
                    registry);

                var thing = project.GetAllMarkdownFiles().Single();
                var text = (await thing.ToHtmlContentAsync()).ToString();
                text.Should().Contain("Hello World!");
            }

            [Fact]
            public async Task Package_option_defaults_to_startup_options()
            {
                var expectedPackage = "console";
                var expectedPackageVersion = "1.2.3";

                var project = new MarkdownProject(
                    new InMemoryDirectoryAccessor(new DirectoryInfo(Directory.GetCurrentDirectory()))
                    {
                        ("readme.md", @"
```cs --source-file Program.cs
```
                        "),
                        ("Program.cs", "")
                    },
                    new PackageRegistry(),
                    new StartupOptions(
                        package: expectedPackage,
                        packageVersion: expectedPackageVersion)
                );

                var html = (await project.GetAllMarkdownFiles()
                                         .Single()
                                         .ToHtmlContentAsync())
                    .ToString();

                html.Should()
                    .Contain($"data-trydotnet-package=\"{expectedPackage}\" data-trydotnet-package-version=\"{expectedPackageVersion}\"");
            }
        }
    }
}