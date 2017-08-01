using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Recipes;
using WorkspaceServer.Models.Completion;
using Xunit;

namespace MLS.Agent.Tests
{
    public class ApiViaHttpTests
    {
        [Fact]
        public async Task When_they_load_a_snippet_then_they_can_use_the_workspace_endpoint_to_compile_their_edited_code()
        {
            var output = Guid.NewGuid().ToString();

            using (var agent = new AgentService())
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

                var response = await agent.SendAsync(request);

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
            using (var agent = new AgentService())
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

                var response = await agent.SendAsync(request);

                var result = await response
                                 .EnsureSuccess()
                                 .DeserializeAs<CompletionResult>();

                result.Items.Should().ContainSingle(item => item.DisplayText == "WriteLine");
            }
        }

        private class CompileResponse
        {
            public bool Succeeded { get; set; }
            public string[] Output { get; set; }
        }
    }
}
