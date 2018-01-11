using System;
using FluentAssertions;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pocket;
using Recipes;
using WorkspaceServer;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace MLS.Agent.Tests
{
    public class ApiViaHttpTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public ApiViaHttpTests(ITestOutputHelper output)
        {
            disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public async Task The_workspace_snippet_endpoint_compiles_code_using_scripting()
        {
            var output = Guid.NewGuid().ToString();
            var code = JsonConvert.SerializeObject(new
            {
                Source = $@"Console.WriteLine(""{output}"");"
            });

            var response = await CallRun(code, "snippet");

            var result = await response
                               .EnsureSuccess()
                               .DeserializeAs<RunResult>();

            VerifySucceeded(result);

            result.ShouldSucceedWithOutput(output);
        }

        [Fact]
        public async Task The_workspace_endpoint_compiles_code_using_dotnet_when_a_workspace_id_is_specified()
        {
            var workspaceId = Guid.NewGuid().ToString("N");
            var registry = new WorkspaceServerRegistry();
            registry.AddWorkspace(workspaceId, o => o.CreateUsingDotnet("console"));
            disposables.Add(registry);

            var output = Guid.NewGuid().ToString("N");
            var code = Create.SimpleRunRequestJson(output);

            var response = await CallRun(code, workspaceId, registry);

            var result = await response
                               .EnsureSuccess()
                               .DeserializeAs<RunResult>();

            VerifySucceeded(result);

            result.ShouldSucceedWithOutput(output);
        }

        [Fact]
        public async Task When_they_load_a_snippet_then_they_get_diagnostics_for_the_first_line()
        {
            var output = Guid.NewGuid().ToString();

            using (var agent = new AgentService())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/snippet/compile")
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            Source = $@"Console.WriteLine(""{output}"""
                        }),
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await agent.SendAsync(request);

                var result = await response
                                 .EnsureSuccess()
                                 .DeserializeAs<RunResult>();

                result.Diagnostics.Should().Contain(d =>
                    d.Start== 56 &&
                    d.End == 56 &&
                    d.Message == ") expected" &&
                    d.Id == "CS1026");
            }
        }

        [Theory]
        [InlineData("{}")]
        public async Task Sending_payloads_that_dont_include_source_strings_results_in_BadRequest(string content)
        {
            var response = await CallRun(content, "snippet");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("{")]
        [InlineData("")]
        [InlineData("garbage 1235")]
        public async Task Sending_payloads_that_cannot_be_deserialized_results_in_BadRequest(string content)
        {
            var response = await CallRun(content, "snippet");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task When_they_load_a_snippet_then_they_can_use_the_workspace_endpoint_to_get_completions()
        {
            using (var agent = new AgentService())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/snippet/getCompletionItems")
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

        private static async Task<HttpResponseMessage> CallRun(
            string content, 
            string workspaceId,
            WorkspaceServerRegistry workspaceServerRegistry = null)
        {
            HttpResponseMessage response;
            using (var agent = new AgentService(workspaceServerRegistry))
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $@"/workspace/{workspaceId}/compile")
                {
                    Content = new StringContent(
                        content,
                        Encoding.UTF8,
                        "application/json")
                };

                response = await agent.SendAsync(request);
            }

            return response;
        }

        private class FailedRunResult : Exception
        {
            internal FailedRunResult(string message) : base(message)
            {
            }
        }

        private void VerifySucceeded(RunResult runResult)
        {
            if (!runResult.Succeeded)
            {
                throw new FailedRunResult(runResult.ToString());
            }
        }
    }
}
