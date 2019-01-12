using FluentAssertions;
using Markdig;
using MLS.Agent.Markdown;
using MLS.Project.Generators;
using System.IO;
using Xunit;
using HtmlAgilityPack;

namespace MLS.Agent.Tests
{
    public class CodeLinkExtensionTests
    {
        [Theory]
        [InlineData("cs")]
        [InlineData("csharp")]
        [InlineData("c#")]
        [InlineData("CS")]
        [InlineData("CSHARP")]
        [InlineData("C#")]
        public void Inserts_code_when_an_existing_file_is_linked(string language)
        {
            var testDir = TestAssets.SampleConsole;
            var fileContent = @"using System;

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
            var directoryAccessor = new InMemoryDirectoryAccessor(testDir)
            {
                 ("Program.cs", fileContent),
                 ("sample.csproj", "")
            };
            var pipeline = new MarkdownPipelineBuilder().UseCodeLinks(directoryAccessor).Build();
            var document =
$@"```{language} Program.cs
```";
            string html = Markdig.Markdown.ToHtml(document, pipeline).EnforceLF();
            html.Should().Contain(fileContent.HtmlEncode());
        }

        [Fact]
        public void Does_not_insert_code_when_specified_language_is_not_csharp()
        {
            string expectedValue =
@"<pre><code class=""language-js"">console.log(&quot;Hello World&quot;);
</code></pre>
".EnforceLF();

            var testDir = TestAssets.SampleConsole;
            var directoryAccessor = new InMemoryDirectoryAccessor(testDir);
            var pipeline = new MarkdownPipelineBuilder().UseCodeLinks(directoryAccessor).Build();
            var document = @"
```js Program.cs
console.log(""Hello World"");
```";
            var html = Markdig.Markdown.ToHtml(document, pipeline).EnforceLF();
            html.Should().Contain(expectedValue);
        }

        [Fact]
        public void Error_messsage_is_displayed_when_the_linked_file_does_not_exist()
        {
            var testDir = TestAssets.SampleConsole;
            var directoryAccessor = new InMemoryDirectoryAccessor(testDir)
            {
                ("sample.csproj", "")
            };
            var pipeline = new MarkdownPipelineBuilder().UseCodeLinks(directoryAccessor).Build();
            var document =
@"```cs DOESNOTEXIST
```";
            var html = Markdig.Markdown.ToHtml(document, pipeline).EnforceLF();
            html.Should().Contain("File not found: DOESNOTEXIST");
        }

        [Fact]
        public void Error_message_is_displayed_when_no_project_is_passed_and_no_project_file_is_found()
        {
            var testDir = TestAssets.SampleConsole;
            var directoryAccessor = new InMemoryDirectoryAccessor(testDir)
            {
                ("Program.cs", "")
            };
            var pipeline = new MarkdownPipelineBuilder().UseCodeLinks(directoryAccessor).Build();
            var document =
@"```cs Program.cs
```";
            var html = Markdig.Markdown.ToHtml(document, pipeline).EnforceLF();

            html.Should().Contain($"No project file could be found at path {testDir.FullName}");
        }


        [Fact(Skip ="Blocked on parser bug")]
        public void Error_message_is_displayed_when_the_passed_project_file_doesnot_exist()
        {
            var testDir = TestAssets.SampleConsole;
            var directoryAccessor = new InMemoryDirectoryAccessor(testDir)
            {
                ("Program.cs", "")
            };
            var projectPath = "sample.csproj";
           
            var document =
$@"```cs --project {projectPath} Program.cs
```";
            var pipeline = new MarkdownPipelineBuilder().UseCodeLinks(directoryAccessor).Build();
            var html = Markdig.Markdown.ToHtml(document, pipeline).EnforceLF();

            html.Should().Contain($"Project not found: {projectPath}");
        }

        [Fact]
        public void Sets_the_trydotnet_project_template_attribute_using_the_passed_project_path()
        {
            var rootDirectory = TestAssets.SampleConsole;
            var currentDir = new DirectoryInfo(Path.Combine(rootDirectory.FullName, "docs"));
            var directoryAccessor = new InMemoryDirectoryAccessor(currentDir, rootDirectory)
            {
                ("src/sample/Program.cs", ""),
                ("src/sample/sample.csproj", "")
            };

            var pipeline = new MarkdownPipelineBuilder().UseCodeLinks(directoryAccessor).Build();

            var projectTemplate = "../src/sample/sample.csproj";
            var document =
$@"```cs --project {projectTemplate} ../src/sample/Program.cs
```";

            var html = Markdig.Markdown.ToHtml(document, pipeline).EnforceLF();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var output = htmlDocument.DocumentNode
                .SelectSingleNode("//pre/code").Attributes["data-trydotnet-project-template"];

            var fullProjectPath = directoryAccessor.GetFullyQualifiedPath(projectTemplate);
            output.Value.Should().Be(fullProjectPath);
        }

        /*[Fact]
        public void Project_template_is_based_on_working_directory_when_project_options_is_not_specified()
        {
            var rootDirectory = TestAssets.SampleConsole;
            var currentDir = new DirectoryInfo(Path.Combine(rootDirectory.FullName, "docs"));
            var directoryAccessor = new InMemoryDirectoryAccessor(currentDir, rootDirectory)
            {
                ("src/sample/Program.cs", ""),
                ("sample.csproj", "")
            };

            var pipeline = new MarkdownPipelineBuilder().UseCodeLinks(directoryAccessor).Build();

            var projectTemplate = "../src/sample/sample.csproj";
            var document =
$@"```cs ../src/sample/Program.cs
```";
                projectTemplate.Value.Should().Be("BasicConsoleApp");
            }
        }*/

        /*[Fact]
        public void When_the_specified_project_template_does_not_exist_then_an_error_message_is_shown()
        {
            using (var agent = new AgentService(new StartupOptions(rootDirectory: TestAssets.SampleConsole)))
            {
                var response = await agent.GetAsync(@"Readme.md");

                response.Should().BeSuccessful();

                var html = await response.Content.ReadAsStringAsync();

                var document = new HtmlDocument();
                document.LoadHtml(html);
                var errorMessage = document.DocumentNode
                    .SelectSingleNode("//pre/code").Attributes["data-trydotnet-error"];

                errorMessage.Value.Should().Be("The specified project template does not exist");
            }
        }

        [Fact]
        public void When_the_specified_project_template_does_not_exist_then_original_fenced_code_is_displayed()
        {
            using (var agent = new AgentService(new StartupOptions(rootDirectory: TestAssets.SampleConsole)))
            {
                var response = await agent.GetAsync(@"Readme.md");

                response.Should().BeSuccessful();

                var html = await response.Content.ReadAsStringAsync();

                var document = new HtmlDocument();
                document.LoadHtml(html);
                var fencedCode = document.DocumentNode
                    .SelectSingleNode("//pre/code").InnerHtml;

                fencedCode.Should().Be("//specify the code");
            }
        }

        [Fact]
        public void Project_template_is_based_the_project_option_when_it_is_specified()
        {
            using (var agent = new AgentService(new StartupOptions(rootDirectory: TestAssets.SampleConsole)))
            {
                var response = await agent.GetAsync(@"Readme.md");

                response.Should().BeSuccessful();

                var html = await response.Content.ReadAsStringAsync();

                var document = new HtmlDocument();
                document.LoadHtml(html);
                var fencedCode = document.DocumentNode
                    .SelectSingleNode("//pre/code").Attributes["data-trydotnet-error"];

                fencedCode.Should().Be("The specified project template does not exist");
            }
        }*/
    }
}
