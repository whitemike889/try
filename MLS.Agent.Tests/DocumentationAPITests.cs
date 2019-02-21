using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HtmlAgilityPack;
using Markdig;
using MLS.Agent.Markdown;
using Recipes;
using WorkspaceServer;
using Xunit;

namespace MLS.Agent.Tests
{
    public class DocumentationAPITests
    {
        [Fact]
        public async Task Request_for_non_existent_markdown_file_returns_404()
        {
            using (var agent = new AgentService(new StartupOptions(rootDirectory: TestAssets.SampleConsole)))
            {
                var response = await agent.GetAsync(@"/DOESNOTEXIST");

                response.Should().BeNotFound();
            }
        }

        [Fact]
        public async Task Return_html_for_an_existing_markdown_file()
        {
            using (var agent = new AgentService(new StartupOptions(rootDirectory: TestAssets.SampleConsole)))
            {
                var response = await agent.GetAsync(@"Readme.md");

                response.Should().BeSuccessful();

                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("<em>markdown file</em>");
            }
        }

        [Fact]
        public async Task Return_html_for_existing_markdown_files_in_a_subdirectory()
        {
            using (var agent = new AgentService(new StartupOptions(rootDirectory: TestAssets.SampleConsole)))
            {
                var response = await agent.GetAsync(@"Subdirectory/Tutorial.md");

                response.Should().BeSuccessful();

                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("<em>tutorial file</em>");
            }
        }

        [Fact]
        public async Task Lists_markdown_files_when_a_folder_is_requested()
        {
            using (var agent = new AgentService(new StartupOptions(rootDirectory: TestAssets.SampleConsole)))
            {
                var response = await agent.GetAsync(@"/");

                response.Should().BeSuccessful();

                var html = await response.Content.ReadAsStringAsync();

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                var links = htmlDoc.DocumentNode
                                   .SelectNodes("//a")
                                   .Select(a => a.Attributes["href"].Value)
                                   .ToArray();

                links.Should().Contain("./Readme.md");
                links.Should().Contain("Subdirectory/Tutorial.md");
            }
        }

        [Fact]
        public async Task Scaffolding_HTML_includes_trydotnet_js_script_link()
        {
            using (var agent = new AgentService(new StartupOptions(rootDirectory: TestAssets.SampleConsole)))
            {
                var response = await agent.GetAsync(@"Subdirectory/Tutorial.md");

                response.Should().BeSuccessful();

                var html = await response.Content.ReadAsStringAsync();

                var document = new HtmlDocument();
                document.LoadHtml(html);

                var script = document.DocumentNode
                                     .Descendants("head")
                                     .Single()
                                     .Descendants("script")
                                     .FirstOrDefault();

                script.Attributes["src"].Value.Should().Be("/api/trydotnet.min.js");
            }
        }

        [Fact]
        public async Task Scaffolding_HTML_includes_trydotnet_js_autoEnable_invocation()
        {
            using (var agent = new AgentService(new StartupOptions(rootDirectory: TestAssets.SampleConsole)))
            {
                var response = await agent.GetAsync(@"Subdirectory/Tutorial.md");

                response.Should().BeSuccessful();

                var html = await response.Content.ReadAsStringAsync();

                var document = new HtmlDocument();
                document.LoadHtml(html);

                var script = document.DocumentNode
                                     .Descendants("body")
                                     .Single()
                                     .Descendants("script")
                                     .FirstOrDefault();

                script.InnerHtml.Should().Be(@"trydotnet.autoEnable(new URL(""http://localhost""));");
            }
        }
    }
}
