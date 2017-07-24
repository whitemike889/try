using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Recipes;
using WorkspaceServer.Models.Completion;
using Xunit;
using Xunit.Abstractions;

namespace MLS.Agent.Tests
{
    public class ApiViaHttpTests
    {
        private readonly ITestOutputHelper output;

        public ApiViaHttpTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        private IWebHostBuilder CreateWebHostBuilder()
        {
            var config = new ConfigurationBuilder().Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<Startup>();

            return host;
        }

        [Fact]
        public async Task When_they_load_a_snippet_then_they_can_use_the_workspace_endpoint_to_compile_their_edited_code()
        {
            var output = Guid.NewGuid().ToString();

            using (var server = CreateTestServer())
            using (var client = server.CreateClient())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/hello/compile")
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            Source = $@"Console.WriteLine(""{output}"");"
                        }),
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await client.SendAsync(request);

                var result = await response
                                 .EnsureSuccess()
                                 .DeserializeAs<CompileResponse>();

                result.Succeeded
                      .Should()
                      .BeTrue();

                result.Output
                      .Should()
                      .ContainSingle(s => s == output);
            }
        }

        [Fact]
        public async Task When_they_load_a_snippet_then_they_can_use_the_workspace_endpoint_to_get_completions()
        {
            var output = Guid.NewGuid().ToString();

            using (var server = CreateTestServer())
            using (var client = server.CreateClient())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/hello/getCompletionItems")
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            Source = "Console.",
                            Position = 8
                        }),
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await client.SendAsync(request);

                var result = await response
                                 .EnsureSuccess()
                                 .DeserializeAs<CompletionResult>();

                result.Items.Should().ContainSingle(item => item.DisplayText == "WriteLine");
            }
        }

        private TestServer CreateTestServer()
        {
            return new TestServer(CreateWebHostBuilder());
        }

        private class CompileResponse
        {
            public bool Succeeded { get; set; }
            public string[] Output { get; set; }
        }
    }
}
