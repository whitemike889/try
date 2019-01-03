using FluentAssertions;
using System.Net;
using System.Net.Http;
using Xunit;
using System.Threading.Tasks;
using System;

namespace MLS.Agent.Tests
{
    public class DocumentationControllerTests
    {
        [Fact]
        public async Task Request_for_non_existent_markdown_file_returns_404()
        {
            using (var agent = new AgentService(new StartupOptions(rootDirectory: TestAssets.BasicConsole)))
            {
                var response = await agent.
                    SendAsync(new HttpRequestMessage(HttpMethod.Get, @"/DOESNOTEXIST"));

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task Return_html_for_existing_markdown_files()
        {
            using (var agent = new AgentService(new StartupOptions(rootDirectory: TestAssets.BasicConsole)))
            {
                var response = await agent.
                    SendAsync(new HttpRequestMessage(HttpMethod.Get, @"Readme.md"));

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("<em>markdown file</em>");
            }
        }

        [Fact]
        public async Task Return_html_for_existing_markdown_files_in_subdirectories()
        {
            using (var agent = new AgentService(new StartupOptions(rootDirectory: TestAssets.BasicConsole)))
            {
                var response = await agent.
                    SendAsync(new HttpRequestMessage(HttpMethod.Get, @"Subdirectory/Tutorial.md"));

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("<em>tutorial file</em>");
            }
        }
    }
}
