using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using MLS.Agent;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Roslyn;
using Xunit;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public class RoslynWorkspaceServerConsoleProjectTests : WorkspaceServerTests
    {
        public RoslynWorkspaceServerConsoleProjectTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override Workspace CreateWorkspaceWithMainContaining(string text)
        {
            return new Workspace(
                $@"using System; using System.Linq; using System.Collections.Generic; class Program {{ static void Main() {{ {text}
                    }}
                }}
            ", workspaceType: GetWorkspaceType());
        }

        protected override string GetWorkspaceType()
        {
            return "console";
        }

        [Fact]
        public async Task When_compile_is_unsuccessful_diagnostic_are_aligned_with_buffer_span()
        {
            var workspace = new Workspace(
                workspaceType: "console",
                files: new[] { new Workspace.File("Program.cs", CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion) },
                buffers: new[] { new Workspace.Buffer("Program.cs@alpha", @"Console.WriteLine(banana);", 0) });
            var server = await GetRunner();

            var result = await server.Run(workspace);

            result.Should().BeEquivalentTo(new
            {
                Succeeded = false,
                Output = new[] { "(1,19): error CS0103: The name \'banana\' does not exist in the current context" },
                Exception = (string)null, // we already display the error in Output
            }, config => config.ExcludingMissingMembers());
        }

        [Fact]
        public async Task When_compile_is_unsuccessful_diagnostic_are_aligned_with_buffer_span_when_code_is_multi_line()
        {
            var workspace = new Workspace(
                workspaceType: "console",
                files: new[] { new Workspace.File("Program.cs", CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion) },
                buffers: new[] { new Workspace.Buffer("Program.cs@alpha", @"var a = 10;" + Environment.NewLine + "Console.WriteLine(banana);", 0) });
            var server = await GetRunner();

            var result = await server.Run(workspace);

            result.Should().BeEquivalentTo(new
            {
                Succeeded = false,
                Output = new[] { "(2,19): error CS0103: The name \'banana\' does not exist in the current context" },
                Exception = (string)null, // we already display the error in Output
            }, config => config.ExcludingMissingMembers());
        }

        [Fact]
        public async Task Get_diagnostics_produces_appropriate_diagnostics_for_display_to_user_when_using_buffers()
        {
            var codeLine1 = @"var a = 10;";
            var codeLine2 = @"Console.WriteLine(banana);";
            var code = $"{codeLine1}{Environment.NewLine}{codeLine2}";
            var erroPos = code.IndexOf("banana);");
            var workspace = new Workspace(
                workspaceType: "console",
                files: new[] { new Workspace.File("Program.cs", CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion) },
                buffers: new[] { new Workspace.Buffer("Program.cs@alpha", code, 0) });
            var request = new WorkspaceRequest(workspace);
            var server = GetLanguageService();

            var result = await server.GetDiagnostics(request.Workspace);

            result.Diagnostics.Should().Contain(d => d.Id == "CS0103" && d.Start == erroPos); // banana is not defined
        }

        [Fact]
        public async Task When_Run_is_called_again_then_previous_file_state_is_cleaned_up()
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

namespace FibonacciTest
{
    public static class FibonacciGenerator
    {
        public  static IEnumerable<int> Fibonacci()
        {
            int current = 1, next = 1;
            while (true)
            {
                yield return current;
                next = current + (current = next);
            }
        }
    }
}";

            #endregion

            var workspace = new Workspace(workspaceType: "console", buffers: new[]
            {
                new Workspace.Buffer("Program.cs", program, 0),
                new Workspace.Buffer("FibonacciGenerator.cs", generator, 0)
            });
            var server = await GetRunner();

            var result = await server.Run(workspace);

            result.Succeeded.Should().BeTrue();

            workspace = new Workspace(workspaceType: "console", buffers: new[]
            {
                new Workspace.Buffer("NotProgram.cs", program, 0),
                new Workspace.Buffer("FibonacciGenerator.cs", generator, 0)
            });
            result = await server.Run(workspace);

            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task Response_with_multi_buffer_workspace()
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

namespace FibonacciTest
{
    public static class FibonacciGenerator
    {
        public  static IEnumerable<int> Fibonacci()
        {
            int current = 1, next = 1;
            while (true)
            {
                yield return current;
                next = current + (current = next);
            }
        }
    }
}";
            #endregion

            var workspace = new Workspace(workspaceType: "console", buffers: new[]
            {
                new Workspace.Buffer("Program.cs",program,0),
                new Workspace.Buffer("FibonacciGenerator.cs",generator,0)
            });
            var server = await GetRunner();

            var result = await server.Run(workspace);

            result.Succeeded.Should().BeTrue();
            result.Output.Count.Should().Be(20);
            result.Output.Should().BeEquivalentTo("1", "1", "2", "3", "5", "8", "13", "21", "34", "55", "89", "144", "233", "377", "610", "987", "1597", "2584", "4181", "6765");
        }

        [Fact]
        public async Task Response_with_multi_buffer_using_relative_paths_workspace()
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

namespace FibonacciTest
{
    public static class FibonacciGenerator
    {
        public  static IEnumerable<int> Fibonacci()
        {
            int current = 1, next = 1;
            while (true)
            {
                yield return current;
                next = current + (current = next);
            }
        }
    }
}";
            #endregion

            var workspace = new Workspace(workspaceType: "console", buffers: new[]
            {
                new Workspace.Buffer("Program.cs",program,0),
                new Workspace.Buffer("generators/FibonacciGenerator.cs",generator,0)
            });

            var server = await GetRunner();

            var result = await server.Run(workspace);

            result.Succeeded.Should().BeTrue();
            result.Output.Count.Should().Be(20);
            result.Output.Should().BeEquivalentTo("1", "1", "2", "3", "5", "8", "13", "21", "34", "55", "89", "144", "233", "377", "610", "987", "1597", "2584", "4181", "6765");
        }

        protected override async Task<ICodeRunner> GetRunner(
            [CallerMemberName] string testName = null)
        {
            return new RoslynWorkspaceServer(new WorkspaceRegistry());
        }

        protected override ILanguageService GetLanguageService(
            [CallerMemberName] string testName = null) => new RoslynWorkspaceServer(
                DefaultWorkspaces.CreateWorkspaceServerRegistry());

    }
}

