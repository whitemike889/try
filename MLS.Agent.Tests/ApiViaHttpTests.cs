using System;
using System.Linq;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Clockwise;
using Newtonsoft.Json;
using Pocket;
using Recipes;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Models.SingatureHelp;
using WorkspaceServer.Tests;
using Xunit;
using Xunit.Abstractions;
using static Pocket.Logger<MLS.Agent.Tests.ApiViaHttpTests>;
using Workspace = WorkspaceServer.Models.Execution.Workspace;

namespace MLS.Agent.Tests
{
    public class ApiViaHttpTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public ApiViaHttpTests(ITestOutputHelper output)
        {
            disposables.Add(output.SubscribeToPocketLogger());
            disposables.Add(VirtualClock.Start());
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public async Task The_workspace_snippet_endpoint_compiles_code_using_scripting_when_a_workspace_type_is_not_specified()
        {
            var output = Guid.NewGuid().ToString();
            var code = JsonConvert.SerializeObject(new
            {
                Buffer = $@"Console.WriteLine(""{output}"");"
            });

            var response = await CallRun(code);

            var result = await response
                               .EnsureSuccess()
                               .DeserializeAs<RunResult>();

            VerifySucceeded(result);

            result.ShouldSucceedWithOutput(output);
        }

        [Fact]
        public async Task The_workspace_snippet_endpoint_compiles_code_using_scripting_when_source_is_specified()
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
                Buffer = $@"Console.WriteLine(""{output}"");",
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
            var output = Guid.NewGuid().ToString();
            var requestJson = Create.SimpleWorkspaceAsJson(output, "console");

            var response = await CallRun(requestJson);

            var result = await response
                                .EnsureSuccess()
                                .DeserializeAs<RunResult>();

            VerifySucceeded(result);

            result.ShouldSucceedWithOutput(output);
        }

        [Fact]
        public async Task When_a_non_script_workspace_type_is_specified_then_code_fragments_cannot_be_compiled_successfully()
        {
            var requestJson = JsonConvert.SerializeObject(new
            {
                Buffer = @"Console.WriteLine(""hello!"");",
                WorkspaceType = "console"
            });

            var response = await CallRun(requestJson);

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
                    @"/workspace/run")
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            Buffer = $@"Console.WriteLine(""{output}"""
                        }),
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await agent.SendAsync(request);

                var result = await response
                                    .EnsureSuccess()
                                    .DeserializeAs<RunResult>();

                result.Diagnostics.Should().Contain(d =>
                                                        d.Start == 56 &&
                                                        d.End == 56 &&
                                                        d.Message == ") expected" &&
                                                        d.Id == "CS1026");
            }
        }

        [Fact]
        public async Task Sending_payloads_that_dont_include_source_strings_results_in_BadRequest()
        {
            var response = await CallRun("{}");

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
                    @"/workspace/completion")
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            workspace = new { workspaceType = "script", buffers = new[] { new { content = "Console.", id = "default.cs" } } },
                            position = 8,
                            activeBufferId = "default.cs"
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

        [Fact]
        public async Task When_they_load_a_snippet_then_they_can_use_the_workspace_endpoint_to_get_signature_help()
        {
            using (var agent = new AgentService())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/signaturehelp")
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            workspace = new { workspaceType = "script", buffers = new[] { new { content = "Console.WriteLine()", id = "default.cs" } } },
                            position = 18,
                            activeBufferId = "default.cs"
                        }),
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await agent.SendAsync(request);

                var result = await response
                    .EnsureSuccess()
                    .DeserializeAs<SignatureHelpResponse>();
                result.Signatures.Should().NotBeNullOrEmpty();
                result.Signatures.Should().Contain(signature => signature.Label == "void Console.WriteLine(string format, params object[] arg)");
            }
        }

        [Fact]
        public async Task When_they_load_a_project_then_they_can_use_the_workspace_endpoint_to_get_signature_help()
        {
            #region bufferSources

            const string program = @"using System;
using System.Linq;

namespace FibonacciTest
{
    public class Program
    {
        public static void Main()
        {
            foreach (var i in FibonacciGenerator.Fibonacci().Take(20))
            {
                Console.WriteLine(i);
            }
        }       
    }
}";
            const string generator = @"using System.Collections.Generic;
using System;
namespace FibonacciTest
{
    public static class FibonacciGenerator
    {
        public static IEnumerable<int> Fibonacci()
        {
            int current = 1, next = 1;
            while (true)
            {
                yield return current;
                next = current + (current = next);
                Console.WriteLine();
            }
        }
    }
}";

            const string consoleWriteline = @"                Console.WriteLine(";
            #endregion

            var position = generator.IndexOf(consoleWriteline, StringComparison.Ordinal) + consoleWriteline.Length;

            using (var agent = new AgentService())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/signaturehelp")
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            workspace = new
                            {
                                workspaceType = "console",
                                buffers = new[]
                            {
                                new { content = program, id = "Program.cs" },
                                new { content = generator, id = "generators/FibonacciGenerator.cs" }
                            }
                            },
                            position = position,
                            activeBufferId = "generators/FibonacciGenerator.cs"
                        }),
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await agent.SendAsync(request);

                var result = await response
                    .EnsureSuccess()
                    .DeserializeAs<SignatureHelpResponse>();
                result.Signatures.Should().NotBeNullOrEmpty();
                result.Signatures.Should().Contain(signature => signature.Label == "void Console.WriteLine(string format, params object[] arg)");
            }
        }

        [Fact]
        public async Task When_they_load_a_project_then_they_can_use_the_workspace_endpoint_to_get_type_completion()
        {
            #region bufferSources

            const string program = @"using System;
using System.Linq;

namespace FibonacciTest
{
    public class Program
    {
        public static void Main()
        {
            foreach (var i in FibonacciGenerator.Fibonacci().Take(20))
            {
                Console.WriteLine(i);
            }
        }       
    }
}";
            const string generator = @"using System.Collections.Generic;
using System;
namespace FibonacciTest
{
    public static class FibonacciGenerator
    {
        public static IEnumerable<int> Fibonacci()
        {
            int current = 1, next = 1;
            while (true)
            {
                yield return current;
                next = current + (current = next);
                Cons
            }
        }
    }
}";

            const string consoleWriteline = @"                Cons";
            #endregion

            var position = generator.IndexOf(consoleWriteline, StringComparison.Ordinal) + consoleWriteline.Length;

            using (var agent = new AgentService())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/completion")
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            workspace = new
                            {
                                workspaceType = "console",
                                buffers = new[]
                            {
                                new { content = program, id = "Program.cs" },
                                new { content = generator, id = "generators/FibonacciGenerator.cs" }
                            }
                            },
                            position = position,
                            activeBufferId = "generators/FibonacciGenerator.cs"
                        }),
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await agent.SendAsync(request);

                var result = await response
                    .EnsureSuccess()
                    .DeserializeAs<CompletionResult>();
                result.Items.Should().NotBeNullOrEmpty();
                result.Items.Should().Contain(completion => completion.SortText == "Console");
            }
        }

        [Fact]
        public async Task When_they_load_a_project_then_they_can_use_the_workspace_endpoint_to_get_method_completion()
        {
            #region bufferSources

            const string program = @"using System;
using System.Linq;

namespace FibonacciTest
{
    public class Program
    {
        public static void Main()
        {
            foreach (var i in FibonacciGenerator.Fibonacci().Take(20))
            {
                Console.WriteLine(i);
            }
        }       
    }
}";
            const string generator = @"using System.Collections.Generic;
using System;
namespace FibonacciTest
{
    public static class FibonacciGenerator
    {
        public static IEnumerable<int> Fibonacci()
        {
            int current = 1, next = 1;
            while (true)
            {
                yield return current;
                next = current + (current = next);
                Console.
            }
        }
    }
}";

            const string consoleWriteline = @"                Console.";
            #endregion

            var position = generator.IndexOf(consoleWriteline, StringComparison.Ordinal) + consoleWriteline.Length;

            using (var agent = new AgentService())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/completion")
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            workspace = new
                            {
                                workspaceType = "console",
                                buffers = new[]
                            {
                                new { content = program, id = "Program.cs" },
                                new { content = generator, id = "generators/FibonacciGenerator.cs" }
                            }
                            },
                            position = position,
                            activeBufferId = "generators/FibonacciGenerator.cs"
                        }),
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await agent.SendAsync(request);

                var result = await response
                    .EnsureSuccess()
                    .DeserializeAs<CompletionResult>();
                result.Items.Should().NotBeNullOrEmpty();
                result.Items.Should().Contain(completion => completion.SortText == "Beep(int frequency, int duration)");
            }
        }

        [Fact]
        public async Task When_invoked_with_workspace_it_executes_correctly()
        {
            var output = "1";
            var requestJson = @"{ ""Buffers"":[{""Id"":"""",""Content"":""using System;\nusing System.Linq;\n\npublic class Program\n{\n  public static void Main()\n  {\n    foreach (var i in Fibonacci().Take(1))\n    {\n      Console.WriteLine(i);\n    }\n  }\n\n  private static IEnumerable<int> Fibonacci()\n  {\n    int current = 1, next = 1;\n\n    while (true) \n    {\n      yield return current;\n      next = current + (current = next);\n    }\n  }\n}\n"",""Position"":0}],""Usings"":[],""WorkspaceType"":""script"",""Files"":[]}";

            var response = await CallRun(requestJson);

            var result = await response
                .EnsureSuccess()
                .DeserializeAs<RunResult>();

            VerifySucceeded(result);

            result.ShouldSucceedWithOutput(output);
        }

        [Fact]
        public async Task When_invoked_with_script_workspace_request_it_executes_correctly()
        {
            var output = "1";
            var sourceCode = @"using System;
using System.Linq;

public class Program {
    public static void Main() {
        foreach (var i in Fibonacci().Take(1)) {
            Console.WriteLine(i);
        }
    }

    private static IEnumerable<int> Fibonacci() {
        int current = 1, next = 1;
        while (true) {
            yield return current;
            next = current + (current = next);
        }
    }
}";
            var request = new WorkspaceRequest(new Workspace(workspaceType: "script", buffers: new[] { new Workspace.Buffer(string.Empty, sourceCode, 0) }));

            var response = await CallRun(request);

            var result = await response
                .EnsureSuccess()
                .DeserializeAs<RunResult>();

            VerifySucceeded(result);

            result.ShouldSucceedWithOutput(output);
        }

        [Fact]
        public async Task When_aspnet_webapi_workspace_request_succeeds_then_output_shows_web_response()
        {
            var workspaceType = Tools.Workspace.Copy(await Default.WebApiWorkspace);
            var workspace = Workspace.FromDirectory(workspaceType.Directory, workspaceType.Directory.Name);

            var request = new WorkspaceRequest(workspace, new HttpRequest("/api/values", "get"));

            var response = await CallRun(request);

            var result = await response
                               .EnsureSuccess()
                               .DeserializeAs<RunResult>();

            Log.Info("output: {x}", result.Output);

            result.ShouldSucceedWithOutput(
                "Status code: 200 OK",
                "Content headers:",
                "  Date:*",
                // the order of these two varies for some reason
                "  *", // e.g. Transfer-Encoding: chunked
                "  *", // e.g. Server: Kestrel
                "  Content-Type: application/json; charset=utf-8",
                "Content:",
                "[",
                "  \"value1\",",
                "  \"value2\"",
                "]");
        }

        [Fact(Skip = "WIP")]
        public async Task When_aspnet_webapi_workspace_request_succeeds_then_standard_out_is_available_on_response()
        {
            var workspaceType = Tools.Workspace.Copy(await Default.WebApiWorkspace);
            var workspace = Workspace.FromDirectory(workspaceType.Directory, workspaceType.Directory.Name);

            var request = new WorkspaceRequest(workspace, new HttpRequest("/api/values", "get"));

            var response = await CallRun(request, 30000);

            var result = await response
                               .EnsureSuccess()
                               .Content
                               .ReadAsStringAsync();

            Log.Info("result: {x}", result);

            throw new NotImplementedException();
        }

        [Fact]
        public async Task When_aspnet_webapi_workspace_request_fails_then_diagnostics_are_returned()
        {
            var workspaceType = Tools.Workspace.Copy(await Default.WebApiWorkspace);
            var workspace = Workspace.FromDirectory(workspaceType.Directory, workspaceType.Directory.Name);
            var nonCompilingBuffer = new Workspace.Buffer("broken.cs", "this does not compile", 0);
            workspace = new Workspace(
                buffers: workspace.Buffers.Concat(new[] { nonCompilingBuffer }).ToArray(),
                files: workspace.Files.ToArray(),
                workspaceType: workspace.WorkspaceType);

            var request = new WorkspaceRequest(workspace, new HttpRequest("/api/values", "get"));

            var response = await CallRun(request);

            var result = await response
                               .EnsureSuccess()
                               .DeserializeAs<RunResult>();

            result.ShouldFailWithOutput("(1,1): error CS1031: Type expected");
        }

        [Fact]
        public async Task When_Run_times_out_in_workspace_server_code_then_the_response_code_is_504()
        {
            Clock.Reset();

            var requestJson =
                @"{ ""Buffers"":[{""Id"":"""",""Content"":""public class Program { public static void Main()\n  {\n  System.Threading.Thread.Sleep(System.TimeSpan.FromTicks(30));  }  }""}],""Usings"":[],""WorkspaceType"":""console""}";

            var response = await CallRun(requestJson, 1);

            response.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
        }

        [Fact]
        public async Task When_Run_times_out_in_user_code_in_a_console_workspace_type_then_the_response_code_is_417()
        {
            // TODO-JOSEQU: (When_Run_times_out_in_user_code_then_the_response_code_is_417) make this test faster

            Clock.Reset();

            var requestJson =
                @"{ ""Buffers"":[{""Id"":"""",""Content"":""public class Program { public static void Main()\n  {\n  System.Threading.Thread.Sleep(System.TimeSpan.FromSeconds(30));  }  }""}],""WorkspaceType"":""console""}";

            var response = await CallRun(requestJson, 15000);

            response.StatusCode.Should().Be(HttpStatusCode.ExpectationFailed);
        }

        [Fact]
        public async Task When_Run_times_out_in_user_code_in_a_script_workspace_type_then_the_response_code_is_417()
        {
            // TODO: (When_Run_times_out_in_user_code_in_a_script_workspace_type_then_the_response_code_is_417) make this test faster

            Clock.Reset();

            var requestJson =
                @"{ ""Buffers"":[{""Id"":"""",""Content"":""public class Program { public static void Main()\n  {\n  System.Threading.Thread.Sleep(System.TimeSpan.FromSeconds(30));  }  }""}],""WorkspaceType"":""script""}";

            var response = await CallRun(requestJson, 15000);

            response.StatusCode.Should().Be(HttpStatusCode.ExpectationFailed);
        }

        private static async Task<HttpResponseMessage> CallRun(
            string content,
            int? runTimeoutMs = null)
        {
            HttpResponseMessage response;
            using (var agent = new AgentService())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/run")
                {
                    Content = new StringContent(
                        content,
                        Encoding.UTF8,
                        "application/json")
                };

                if (runTimeoutMs != null)
                {
                    request.Headers.Add("Timeout", runTimeoutMs.Value.ToString("F0"));
                }

                response = await agent.SendAsync(request);
            }

            return response;
        }

        private static Task<HttpResponseMessage> CallRun(
            WorkspaceRequest request,
            int? runTimeoutMs = null)
        {
            return CallRun(request.ToJson(), runTimeoutMs);
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
