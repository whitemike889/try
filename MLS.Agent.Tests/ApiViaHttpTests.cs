using System;
using FluentAssertions;
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
            disposables.Add(output.SubscribeToPocketLogger());
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public async Task The_workspace_snippet_endpoint_compiles_code_using_scripting_when_a_workspace_type_is_not_specified()
        {
            var output = Guid.NewGuid().ToString();
            var code = JsonConvert.SerializeObject(new
            {
                Source = $@"Console.WriteLine(""{output}"");"
            });

            var response = await CallRun(code);

            var result = await response
                               .EnsureSuccess()
                               .DeserializeAs<RunResult>();

            VerifySucceeded(result);

            result.ShouldSucceedWithOutput(output);
        }

        [Fact]
        public async Task The_workspace_snippet_endpoint_compiles_code_using_scripting_when_a_workspace_type_is_specified_as_script()
        {
            var output = Guid.NewGuid().ToString();
            var requestJson = JsonConvert.SerializeObject(new
            {
                Source = $@"Console.WriteLine(""{output}"");",
                WorkspaceType = "script"
            });

            var response = await CallRun(requestJson);

            var result = await response
                               .EnsureSuccess()
                               .DeserializeAs<RunResult>();

            VerifySucceeded(result);

            result.ShouldSucceedWithOutput(output);
        }

        [Fact]
        public async Task The_workspace_endpoint_compiles_code_using_dotnet_when_a_non_script_workspace_type_is_specified()
        {
            var registry = new WorkspaceServerRegistry();
            registry.AddWorkspace("console", o => o.CreateUsingDotnet("console"));
            disposables.Add(registry);

            var output = Guid.NewGuid().ToString();
            var requestJson = Create.SimpleRunRequestJson(output, "console");

            var response = await CallRun(requestJson, registry);

            var result = await response
                               .EnsureSuccess()
                               .DeserializeAs<RunResult>();

            VerifySucceeded(result);

            result.ShouldSucceedWithOutput(output);
        }

        [Fact]
        public async Task When_a_non_script_workspace_type_is_specified_then_code_fragments_cannot_be_compiled_successfully()
        {
            var registry = new WorkspaceServerRegistry();
            registry.AddWorkspace("console", o => o.CreateUsingDotnet("console"));
            disposables.Add(registry);

            var requestJson = JsonConvert.SerializeObject(new
            {
                Source = @"Console.WriteLine(""hello!"");",
                WorkspaceType = "console"
            });

            var response = await CallRun(requestJson, registry);

            var result = await response
                               .EnsureSuccess()
                               .DeserializeAs<RunResult>();

            result.ShouldFailWithOutput(
                "(1,19): error CS1022: Type or namespace definition, or end-of-file expected",
                "(1,19): error CS1026: ) expected",
                "(1,1): error CS5001: Program does not contain a static 'Main' method suitable for an entry point"
            );
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
            var response = await CallRun(content);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("{")]
        [InlineData("")]
        [InlineData("garbage 1235")]
        public async Task Sending_payloads_that_cannot_be_deserialized_results_in_BadRequest(string content)
        {
            var response = await CallRun(content);

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
            WorkspaceServerRegistry workspaceServerRegistry = null)
        {
            HttpResponseMessage response;
            using (var agent = new AgentService(workspaceServerRegistry))
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/snippet/compile")
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
