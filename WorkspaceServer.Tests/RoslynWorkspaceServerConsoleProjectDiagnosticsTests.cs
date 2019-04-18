using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.DotNet.Try.Protocol;
using WorkspaceServer.Servers.Roslyn;
using Xunit;
using Xunit.Abstractions;
using Buffer = Microsoft.DotNet.Try.Protocol.Buffer;
using Package = WorkspaceServer.Packaging.Package;

namespace WorkspaceServer.Tests
{
    public class RoslynWorkspaceServerConsoleProjectDiagnosticsTests : WorkspaceServerTestsCore
    {
        public RoslynWorkspaceServerConsoleProjectDiagnosticsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task Get_diagnostics_with_buffer_with_region()
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
            #region code
            #endregion
        }
    }
}".EnforceLF();

            var region = @"adddd".EnforceLF();

            #endregion

            var (processed, position) = CodeManipulation.ProcessMarkup(region);

            var workspace = new Workspace(workspaceType: "console", buffers: new[]
            {
                new Buffer("Program.cs", program),
                new Buffer("snippets/code.cs@code", processed, position)
            });

            var request = new WorkspaceRequest(workspace, activeBufferId: "snippets/code.cs@code");
            var server = GetLanguageService();
            var result = await server.GetDiagnostics(request);

            result.Diagnostics.Should().NotBeNullOrEmpty();
            result.Diagnostics.Should().Contain(diagnostics => diagnostics.Message == "generators/FibonacciGenerator.cs(14,17): error CS0246: The type or namespace name \'adddd\' could not be found (are you missing a using directive or an assembly reference?)");
        }

        [Fact]
        public async Task Get_diagnostics_with_buffer_with_region_in_code_but_not_in_buffer_id()
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
            #region code
            error
            #endregion
            moreError
        }
    }
}".EnforceLF();

            #endregion

            var (processed, position) = CodeManipulation.ProcessMarkup(program);

            var workspace = new Workspace(workspaceType: "console", buffers: new[]
            {
                new Buffer("Program.cs", program),
            });

            var request = new WorkspaceRequest(workspace, activeBufferId: "Program.cs");
            var server = GetLanguageService();
            var result = await server.GetDiagnostics(request);

            result.Diagnostics.Should().NotBeNullOrEmpty();
            result.Diagnostics.Should().Contain(diagnostics => diagnostics.Message == "generators/FibonacciGenerator.cs(14,17): error CS0246: The type or namespace name \'adddd\' could not be found (are you missing a using directive or an assembly reference?)");
        }

        [Fact]
        public async Task Get_diagnostics()
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
                adddd
                Console.WriteLine($$);
            }
        }
    }
}".EnforceLF();

            #endregion

            var (processed, position) = CodeManipulation.ProcessMarkup(generator);

            var workspace = new Workspace(workspaceType: "console", buffers: new[]
            {
                new Buffer("Program.cs", program),
                new Buffer("generators/FibonacciGenerator.cs", processed, position)
            });

            var request = new WorkspaceRequest(workspace, activeBufferId: "generators/FibonacciGenerator.cs");
            var server = GetLanguageService();
            var result = await server.GetDiagnostics(request);

            result.Diagnostics.Should().NotBeNullOrEmpty();
            result.Diagnostics.Should().Contain(diagnostics => diagnostics.Message == "generators/FibonacciGenerator.cs(14,17): error CS0246: The type or namespace name \'adddd\' could not be found (are you missing a using directive or an assembly reference?)");
        }

        protected override Task<(ICodeRunner runner, Package workspace)> GetRunnerAndWorkspaceBuild(
            [CallerMemberName] string testName = null)
        {
            throw new NotImplementedException();
        }

        protected override ILanguageService GetLanguageService([CallerMemberName] string testName = null)
        {
            return new RoslynWorkspaceServer(PackageRegistry.CreateForHostedMode());
        }
    }
}