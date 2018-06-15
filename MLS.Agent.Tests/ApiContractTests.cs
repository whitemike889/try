using System;
using System.Threading.Tasks;
using Assent;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace MLS.Agent.Tests
{
    public class ApiContractTests : ApiViaHttpTestsBase
    {
        private readonly Configuration configuration;

        public ApiContractTests(ITestOutputHelper output) : base(output)
        {
            configuration = new Configuration()
                            .UsingExtension("json");

#if !DEBUG
            configuration = configuration.SetInteractive(false);
#endif
        }

        [Fact]
        public async Task The_Run_contract_for_compiling_code_has_not_been_broken()
        {
            var viewport = ViewportCode();

            var requestJson = new WorkspaceRequest(
                new Workspace(
                    workspaceType: "console",
                    buffers: new[]
                    {
                        EntrypointCode(),
                        viewport
                    }),
                activeBufferId: viewport.Id,
                position: viewport.Position);

            var response = await CallRun(requestJson);

            var result = await response.Content.ReadAsStringAsync();

            this.Assent(result.FormatJson(), configuration);
        }

        [Fact]
        public async Task The_Run_contract_for_noncompiling_code_has_not_been_broken()
        {
            var viewport = ViewportCode("doesn't compile");

            var requestJson = new WorkspaceRequest(
                new Workspace(
                    workspaceType: "console",
                    buffers: new[]
                    {
                        EntrypointCode(),
                        viewport
                    }),
                activeBufferId: viewport.Id,
                position: viewport.Position);

            var response = await CallRun(requestJson);

            var result = await response.Content.ReadAsStringAsync();

            this.Assent(result.FormatJson(), configuration);
        }

        private static Workspace.Buffer EntrypointCode(string mainContent = @"Console.WriteLine(Sample.Method());$$")
        {
            var input = $@"
using System;
using System.Linq;

namespace Example
{{
    public class Program
    {{
        public static void Main()
        {{
            {mainContent}
        }}       
    }}
}}";

            MarkupTestFile.GetPosition(input, out string output, out var position);

            return new Workspace.Buffer(
                "EntrypointCode.cs",
                output,
                position ?? 0);
        }

        private static Workspace.Buffer ViewportCode(string methodContent = @"return ""Hello world!"";$$ ")
        {
            var input = $@"
using System.Collections.Generic;
using System;

namespace Example
{{
    public static class Sample
    {{
        public static object Method()
        {{
#region viewport
            {methodContent}
#endregion
        }}
    }}
}}";

            MarkupTestFile.GetPosition(input, out string output, out var position);

            return new Workspace.Buffer(
                "ViewportCode.cs",
                output,
                position ?? 0);
        }
    }

}
