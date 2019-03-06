using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Clockwise;
using FluentAssertions;
using Humanizer;
using MLS.Project.Transformations;
using MLS.Protocol;
using MLS.Protocol.Completion;
using MLS.Protocol.Diagnostics;
using MLS.Protocol.Execution;
using MLS.Protocol.SignatureHelp;
using Pocket;
using Recipes;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Tests;
using WorkspaceServer.Packaging;
using Xunit;
using Xunit.Abstractions;
using static Pocket.Logger<MLS.Agent.Tests.ApiViaHttpTests>;
using Workspace = MLS.Protocol.Execution.Workspace;
using MLS.Agent.CommandLine;

namespace MLS.Agent.Tests
{
    public class ApiViaHttpTests : ApiViaHttpTestsBase
    {
        public ApiViaHttpTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task The_workspace_snippet_endpoint_compiles_code_using_scripting_when_a_workspace_type_is_specified_as_script()
        {
            var output = Guid.NewGuid().ToString();

            var requestJson = new WorkspaceRequest(
                Workspace.FromSource(
                    source: $@"Console.WriteLine(""{output}"");".EnforceLF(),
                    workspaceType: "script"
                ),
                requestId: "TestRun").ToJson();

            var response = await CallRun(requestJson);

            var result = await response
                               .EnsureSuccess()
                               .DeserializeAs<RunResult>();

            VerifySucceeded(result);

            result.ShouldSucceedWithOutput(output);
        }

        [Fact]
        public async Task The_compile_endpoint_returns_bad_request_if_workspace_type_is_scripting()
        {
            var output = Guid.NewGuid().ToString();

            var requestJson = new WorkspaceRequest(
                Workspace.FromSource(
                    source: $@"Console.WriteLine(""{output}"");".EnforceLF(),
                    workspaceType: "script"
                ), 
                requestId: "TestRun").ToJson();

            var response = await CallCompile(requestJson);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task The_workspace_endpoint_compiles_code_using_dotnet_when_a_non_script_workspace_type_is_specified()
        {
            var output = Guid.NewGuid().ToString();
            var requestJson = Create.SimpleWorkspaceRequestAsJson(output, "console");

            var response = await CallRun(requestJson);

            var result = await response
                                .EnsureSuccess()
                                .DeserializeAs<RunResult>();

            VerifySucceeded(result);

            result.ShouldSucceedWithOutput(output);
        }

        [Fact]
        public async Task The_workspace_endpoint_will_prevent_compiling_if_is_in_language_service_mode()
        {
            var output = Guid.NewGuid().ToString();
            var requestJson = Create.SimpleWorkspaceRequestAsJson(output, "console");

            var response = await CallRun(requestJson, options: new StartupOptions(true, true, string.Empty));
          
            response.Should().BeNotFound();
        }

        [Fact]
        public async Task When_a_non_script_workspace_type_is_specified_then_code_fragments_cannot_be_compiled_successfully()
        {
            var requestJson =
                new WorkspaceRequest(
                    Workspace.FromSource(
                        @"Console.WriteLine(""hello!"");",
                        workspaceType: "console", 
                        id: "Program.cs")).ToJson();

            var response = await CallRun(requestJson);

            var result = await response
                               .EnsureSuccess()
                               .DeserializeAs<RunResult>();

            result.ShouldFailWithOutput(
                "Program.cs(1,19): error CS1022: Type or namespace definition, or end-of-file expected",
                "Program.cs(1,19): error CS1026: ) expected"
            );
        }

        [Fact]
        public async Task When_they_run_a_snippet_then_they_get_diagnostics_for_the_first_line()
        {
            var output = Guid.NewGuid().ToString();

            using (var agent = new AgentService())
            {
                var json =
                    new WorkspaceRequest(
                            Workspace.FromSource(
                                $@"Console.WriteLine(""{output}""".EnforceLF(),
                                workspaceType: "script"),
                            requestId: "TestRun")
                        .ToJson();

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/run")
                {
                    Content = new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await agent.SendAsync(request);

                var result = await response
                                   .EnsureSuccess()
                                   .DeserializeAs<RunResult>();

                var diagnostics = result.GetFeature<Diagnostics>();

                diagnostics.Should().Contain(d =>
                                                 d.Start == 56 &&
                                                 d.End == 56 &&
                                                 d.Message == "(1,57): error CS1026: ) expected" &&
                                                 d.Id == "CS1026");
            }
        }

        [Theory]
        [InlineData("{}")]
        [InlineData("{ \"workspace\" : { } }")]
        [InlineData( /* has top-level position property */
            "{\r\n  \"workspace\": {\r\n    \"workspaceType\": \"console\",\r\n    \"files\": [],\r\n    \"buffers\": [\r\n      {\r\n        \"id\": \"\",\r\n        \"content\": \"\",\r\n        \"position\": 0\r\n      }\r\n    ],\r\n    \"usings\": []\r\n  },\r\n  \"activeBufferId\": \"\",\r\n  \"position\": 187\r\n}", Skip = "This is still supported")]
        [InlineData( /* buffers array is empty */
            "{\r\n  \"workspace\": {\r\n    \"workspaceType\": \"console\",\r\n    \"files\": [],\r\n    \"buffers\": [],\r\n    \"usings\": []\r\n  },\r\n  \"activeBufferId\": \"\"\r\n}")]
        [InlineData( /* no buffers property */
            "{\r\n  \"workspace\": {\r\n    \"workspaceType\": \"console\",\r\n    \"files\": [],\r\n    \"usings\": []\r\n  },\r\n  \"activeBufferId\": \"\"\r\n}")]
        public async Task Sending_payload_that_deserialize_to_invalid_workspace_objects_results_in_BadRequest(string workspaceRequestBody)
        {
            var response = await CallRun(workspaceRequestBody);

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
        public async Task A_script_snippet_workspace_can_be_used_to_get_completions()
        {
            var (processed, position) = CodeManipulation.ProcessMarkup("Console.$$");
            using (var agent = new AgentService())
            {
                var json = new WorkspaceRequest(
                        requestId: "TestRun",
                        activeBufferId: "default.cs",
                        workspace: Workspace.FromSource(
                            processed,
                            "script",
                            id: "default.cs",
                            position: position))
                    .ToJson();

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/completion")
                {
                    Content = new StringContent(
                        json,
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
        public async Task A_script_snippet_workspace_can_be_used_to_get_signature_help()
        {
            var log = new LogEntryList();
            var (processed, position) = CodeManipulation.ProcessMarkup("Console.WriteLine($$)");
            using (LogEvents.Subscribe(log.Add))
            using (var agent = new AgentService())
            {
                var json = new WorkspaceRequest(
                        requestId: "TestRun",
                        activeBufferId: "default.cs",
                        workspace: Workspace.FromSource(
                            processed,
                            "script",
                            id: "default.cs",
                            position: position))
                    .ToJson();
                
                var request = new HttpRequestMessage(HttpMethod.Post, @"/workspace/signaturehelp")
                {
                    Content = new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await agent.SendAsync(request);

                var result = await response
                                   .EnsureSuccess()
                                   .DeserializeAs<SignatureHelpResult>();
                result.Signatures.Should().NotBeNullOrEmpty();
                result.Signatures.Should().Contain(signature => signature.Label == "void Console.WriteLine(string format, params object[] arg)");
            }
        }

        [Fact]
        public async Task A_script_snippet_workspace_can_be_used_to_get_diagnostics()
        {
            var log = new LogEntryList();
            var (processed, position) = CodeManipulation.ProcessMarkup("adddd");
            using (LogEvents.Subscribe(log.Add))
            using (var agent = new AgentService())
            {
                var json = new WorkspaceRequest(
                        requestId: "TestRun",
                        activeBufferId: "default.cs",
                        workspace: Workspace.FromSource(
                            processed,
                            "script",
                            id: "default.cs",
                            position: position))
                    .ToJson();

                var request = new HttpRequestMessage(HttpMethod.Post, @"/workspace/diagnostics")
                {
                    Content = new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await agent.SendAsync(request);

                var result = await response
                    .EnsureSuccess()
                    .DeserializeAs<DiagnosticResult>();
                result.Diagnostics.Should().NotBeNullOrEmpty();
                result.Diagnostics.Should().Contain(signature => signature.Message == "default.cs(1,1): error CS0103: The name \'adddd\' does not exist in the current context");
            }
        }

        [Fact]
        public async Task A_console_workspace_can_be_used_to_get_signature_help()
        {
            #region bufferSources

            var program = @"using System;
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
}".EnforceLF();
            var generator = @"using System.Collections.Generic;
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
                Console.WriteLine($$);
            }
        }
    }
}".EnforceLF();
            #endregion

            var (processed, position) = CodeManipulation.ProcessMarkup(generator);
            var log = new LogEntryList();
            using (LogEvents.Subscribe(log.Add))
            using (var agent = new AgentService())
            {
                var json =
                    new WorkspaceRequest(activeBufferId: "generators/FibonacciGenerator.cs",
                                         requestId: "TestRun",
                                         workspace: Workspace.FromSources(
                                             "console",
                                             ("Program.cs", program, 0),
                                             ("generators/FibonacciGenerator.cs", processed, position)
                                         )).ToJson();

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/signaturehelp")
                {
                    Content = new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await agent.SendAsync(request);

                var result = await response
                                   .EnsureSuccess()
                                   .DeserializeAs<SignatureHelpResult>();
                result.Signatures.Should().NotBeNullOrEmpty();
                result.Signatures.Should().Contain(diagnostic => diagnostic.Label == "void Console.WriteLine(string format, params object[] arg)");
            }
        }

        [Fact]
        public async Task A_console_project_can_be_used_to_get_type_completion()
        {
            #region bufferSources

            var program = @"using System;
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
}".EnforceLF();

            var generator = @"using System.Collections.Generic;
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
                Cons$$
            }
        }
    }
}".EnforceLF();

            #endregion

            var (processed, position) = CodeManipulation.ProcessMarkup(generator);
            var log = new LogEntryList();
            using (LogEvents.Subscribe(log.Add))
            using (var agent = new AgentService())
            {
                var json =
                    new WorkspaceRequest(activeBufferId: "generators/FibonacciGenerator.cs",
                                        requestId: "TestRun",
                                         workspace: Workspace.FromSources(
                                             "console",
                                             ("Program.cs", program, 0),
                                             ("generators/FibonacciGenerator.cs", processed, position)
                                         )).ToJson();

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/completion")
                {
                    Content = new StringContent(
                        json,
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
        public async Task A_console_project_can_be_used_to_get_diagnostics()
        {
            #region bufferSources

            var program = @"using System;
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
}".EnforceLF();

            var generator = @"using System.Collections.Generic;
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
                adddd
                yield return current;
                next = current + (current = next);
                Cons$$
            }
        }
    }
}".EnforceLF();

            #endregion

            var (processed, position) = CodeManipulation.ProcessMarkup(generator);
            var log = new LogEntryList();
            using (LogEvents.Subscribe(log.Add))
            using (var agent = new AgentService())
            {
                var json =
                    new WorkspaceRequest(activeBufferId: "generators/FibonacciGenerator.cs",
                                        requestId: "TestRun",
                                         workspace: Workspace.FromSources(
                                             "console",
                                             ("Program.cs", program, 0),
                                             ("generators/FibonacciGenerator.cs", processed, position)
                                         )).ToJson();

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/diagnostics")
                {
                    Content = new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await agent.SendAsync(request);

                var result = await response
                    .EnsureSuccess()
                    .DeserializeAs<DiagnosticResult>();
                result.Diagnostics.Should().NotBeNullOrEmpty();
                result.Diagnostics.Should().Contain(diagnostic => diagnostic.Message == "generators/FibonacciGenerator.cs(12,17): error CS0246: The type or namespace name \'adddd\' could not be found (are you missing a using directive or an assembly reference?)");
            }
        }

        [Fact]
        public async Task When_aspnet_webapi_workspace_request_succeeds_then_output_shows_web_response()
        {
            var workspaceType = await Create.WebApiWorkspaceCopy();
            var workspace = WorkspaceFactory.CreateWorkspaceFromDirectory(
                workspaceType.Directory, 
                workspaceType.Directory.Name);

            var request = new WorkspaceRequest(workspace, httpRequest: new HttpRequest("/api/values", "get"), requestId: "TestRun");

            var json = request.ToJson();

            var response = await CallRun(json);

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
            var package = await Package.Copy(await Default.WebApiWorkspace);
            await package.CreateRoslynWorkspaceForRunAsync(new TimeBudget(1.Minutes()));
            var workspace = WorkspaceFactory.CreateWorkspaceFromDirectory(package.Directory, package.Directory.Name);

            var request = new WorkspaceRequest(workspace, httpRequest: new HttpRequest("/api/values", "get"), requestId: "TestRun");

            var response = await CallRun(request.ToJson(), 30000);

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
            var package = await Package.Copy(await Default.WebApiWorkspace);
            await package.CreateRoslynWorkspaceForRunAsync(new TimeBudget(1.Minutes()));
            var workspace = WorkspaceFactory.CreateWorkspaceFromDirectory(package.Directory, package.Directory.Name);
            var nonCompilingBuffer = new Workspace.Buffer("broken.cs", "this does not compile", 0);
            workspace = new Workspace(
                buffers: workspace.Buffers.Concat(new[] { nonCompilingBuffer }).ToArray(),
                files: workspace.Files.ToArray(),
                workspaceType: workspace.WorkspaceType);

            var request = new WorkspaceRequest(workspace, httpRequest: new HttpRequest("/api/values", "get"), requestId: "TestRun");

            var response = await CallRun(request.ToJson(), null);

            var result = await response
                               .EnsureSuccess()
                               .DeserializeAs<RunResult>();

            result.ShouldFailWithOutput("broken.cs(1,1): error CS1031: Type expected");
        }

        [Theory]
        [InlineData(@"
            public class Program { public static void Main()\n  {\n  Console.WriteLine();  }  }")]
        public async Task When_Run_times_out_in_console_workspace_server_code_then_the_response_code_is_504(string code)
        {
            var workspace = Workspace.FromSource(code.EnforceLF(), (await Create.ConsoleWorkspaceCopy()).Name);

            var requestJson = new WorkspaceRequest(workspace).ToJson();

            ((VirtualClock) Clock.Current).OnBudgetEntryRecorded((virtualClock, budget, entry) =>
            {
                Log.Info("Budget entry created: {entry}", entry);

                if (entry.Name == typeof(BufferInliningTransformer).Name)
                {
                    budget.Cancel();
                }
            });

            var response = await CallRun(requestJson);

            response.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
        }

        [Theory]
        [InlineData( @"
            Console.WriteLine();")]
        [InlineData(@"
            public class Program { public static void Main()\n  {\n  Console.WriteLine();  }  }")]
        public async Task When_Run_times_out_in_script_workspace_server_code_then_the_response_code_is_504(string code)
        {
            var workspace = Workspace.FromSource(code.EnforceLF(), "script");

            var requestJson = new WorkspaceRequest(workspace).ToJson();

            ((VirtualClock) Clock.Current).OnBudgetEntryRecorded((virtualClock, budget, entry) =>
            {
                Log.Info("Budget entry created: {entry}", entry);

                budget.Cancel();
            });

            var response = await CallRun(requestJson);

            response.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
        }

        [Theory]
        [InlineData(
            "console",
            @"  using System;
                using System.Threading;
                public class Program 
                { 
                    public static void Main()
                    {
                        Console.WriteLine(""start user code."");
                        Thread.Sleep(30000);  
                        Console.WriteLine(""end user code."");
                    }  
                }")]
        [InlineData(
            "script",
            @"Console.WriteLine(""start user code."");
              System.Threading.Thread.Sleep(30000);
              Console.WriteLine(""end user code."");")]
        [InlineData(
            "script",
            @"  public class Program 
                { 
                    public static void Main()
                    {
                        Console.WriteLine(""start user code."");
                        System.Threading.Thread.Sleep(30000);  
                        Console.WriteLine(""end user code."");
                    }  
                }")]
        public async Task When_Run_times_out_in_user_code_then_the_response_code_is_417(
            string workspaceType,
            string code)
        {
            Clock.Reset();

            var workspace =
                workspaceType == "script"
                    ? Workspace.FromSource(code, "script")
                    : Workspace.FromSource(code, (await Create.ConsoleWorkspaceCopy()).Name);

            var requestJson = new WorkspaceRequest(workspace).ToJson();
            var response = await CallRun(requestJson, 10000);

            Log.Info("{response}", await response.Content.ReadAsStringAsync());

            response.StatusCode.Should().Be(HttpStatusCode.ExpectationFailed);
        }


        [Fact]
        public async Task Can_serve_blazor_code_runner()
        {
            using (var agent = new AgentService())
            {
                var response = await agent.GetAsync(@"/LocalCodeRunner/blazor-console");

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("Loading...");
            }
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
