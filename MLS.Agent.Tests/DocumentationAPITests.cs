using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HtmlAgilityPack;
using Recipes;
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
        public async Task Return_html_for_existing_markdown_files()
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
        public async Task Return_html_for_existing_markdown_files_in_subdirectories()
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

                script.Attributes["src"].Value.Should().Be("//trydotnet.microsoft.com/api/trydotnet.min.js");
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
