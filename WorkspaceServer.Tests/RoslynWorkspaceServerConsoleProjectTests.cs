using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.Text;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Models.Instrumentation;
using WorkspaceServer.Servers.Roslyn;
using WorkspaceServer.Servers.Roslyn.Instrumentation;
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
            return Workspace.FromSource(
                $@"using System; using System.Linq; using System.Collections.Generic; class Program {{ static void Main() {{ {text}
                    }}
                }}
            ",
                workspaceType: GetWorkspaceType());
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

            var result = await server.Run(new WorkspaceRequest(workspace));

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

            var result = await server.Run(new WorkspaceRequest(workspace));

            result.Should().BeEquivalentTo(new
            {
                Succeeded = false,
                Output = new[] { "(2,19): error CS0103: The name \'banana\' does not exist in the current context" },
                Exception = (string)null, // we already display the error in Output
            }, config => config.ExcludingMissingMembers());
        }

        [Fact]
        public async Task When_diagnostics_are_outside_of_viewport_then_they_are_omitted()
        {
            var workspace = new Workspace(
                workspaceType: "console",
                files: new[] { new Workspace.File("Program.cs", CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegionExtraUsing) },
                buffers: new[] { new Workspace.Buffer("Program.cs@alpha", @"var a = 10;" + Environment.NewLine + "Console.WriteLine(a);", 0) });
            var server = await GetRunner();

            var result = await server.Run(new WorkspaceRequest(workspace));

            result.Should().BeEquivalentTo(new
            {
                Succeeded = true,
                Output = new[] { "10", "" },
                Exception = (string)null, // we already display the error in Output
            }, config => config.ExcludingMissingMembers());
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

            var result = await server.Run(new WorkspaceRequest(workspace));

            result.Succeeded.Should().BeTrue();

            workspace = new Workspace(workspaceType: "console", buffers: new[]
            {
                new Workspace.Buffer("NotProgram.cs", program, 0),
                new Workspace.Buffer("FibonacciGenerator.cs", generator, 0)
            });
            result = await server.Run(new WorkspaceRequest(workspace));

            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task Response_with_multi_buffer_workspace_with_instrumentation()
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

            var request = new WorkspaceRequest(
                new Workspace(
                    workspaceType: "console", buffers: new[]
                    {
                        new Workspace.Buffer("Program.cs", program, 0),
                        new Workspace.Buffer("FibonacciGenerator.cs", generator, 0)
                    }, 
                    includeInstrumentation: true),
                new BufferId("Program.cs"));
            var server = await GetRunner();

            var result = await server.Run(request);

            result.Succeeded.Should().BeTrue();
            result.Output.Count.Should().Be(21);
            result.Output.Should().BeEquivalentTo("1", "1", "2", "3", "5", "8", "13", "21", "34", "55", "89", "144", "233", "377", "610", "987", "1597", "2584", "4181", "6765", "");
        }

        [Fact]
        public async Task When_Run_is_called_with_instrumentation_and_no_regions_lines_are_not_mapped()
        {
            var markedUpCode = @"
using System;

namespace ConsoleProgram
{
    public class Program
    {
        public static void Main(string[] args)
        {
            {|line:Console.WriteLine(""test"");|}
            {|line:var a = 10;|}
        }
    }
}";
            MarkupTestFile.GetNamedSpans(markedUpCode, out var code, out var spans);

            var linePositionSpans = ToLinePositionSpan(spans, code);

            var workspace = new Workspace(
                workspaceType: "console",
                buffers: new[] { new Workspace.Buffer("test.cs", code)},
                includeInstrumentation: true
                );

            var server = await GetRunner();
            var result = await server.Run(new WorkspaceRequest(workspace));
            var filePositions = result.Features[typeof(ProgramStateAtPositionArray)].As<ProgramStateAtPositionArray>()
                .ProgramStates
                .Where(state => state.FilePosition != null)
                .Select(state => state.FilePosition.Line);

            var expectedLines = linePositionSpans["line"].Select(loc => loc.Start.Line);

            filePositions.Should().BeEquivalentTo(expectedLines);
        }

        [Fact]
        public async Task When_Run_is_called_with_instrumentation_and_no_regions_variable_locations_are_not_mapped()
        {
            var markedUpCode = @"
using System;

namespace ConsoleProgram
{
    public class Program
    {
        public static void Main(string[] args)
        {
            {|a:var a = 10;|}
            Console.WriteLine({|a:a|});
        }
    }
}";
            MarkupTestFile.GetNamedSpans(markedUpCode, out var code, out var spans);

            var linePositionSpans = ToLinePositionSpan(spans, code);

            var workspace = new Workspace(
                workspaceType: "console",
                buffers: new[] { new Workspace.Buffer("test.cs", code)},
                includeInstrumentation: true
                );

            var server = await GetRunner();
            var result = await server.Run(new WorkspaceRequest(workspace));

            var locations = result.Features[typeof(ProgramDescriptor)].As<ProgramDescriptor>()
                .VariableLocations
                .Where(variable => variable.Name == "a")
                .SelectMany(variable => variable.Locations)
                .Select(location => location.StartLine);
            var expectedLocations = linePositionSpans["a"].Select(loc => loc.Start.Line);

            locations.Should().BeEquivalentTo(expectedLocations);
        }

        [Fact]
        public async Task When_Run_is_called_with_instrumentation_and_regions_variable_locations_are_mapped()
        {
            var code = @"
using System;

namespace ConsoleProgram
{
    public class Program
    {
        public static void Main(string[] args)
        {
        Console.WriteLine();
#region reg
#endregion
        Console.WriteLine(a);
        }
    }
}";
            var regionCodeWithMarkup = "{|a:var a = 10;|}";
            MarkupTestFile.GetNamedSpans(regionCodeWithMarkup, out var regionCode, out var spans);
            var linePositionSpans = ToLinePositionSpan(spans, regionCode);

            var workspace = new Workspace(
                workspaceType: "console",
                buffers: new[] { new Workspace.Buffer("test.cs@reg", regionCode) },
                files: new [] { new Workspace.File("test.cs", code) },
                includeInstrumentation: true
                );

            var server = await GetRunner();
            var result = await server.Run(new WorkspaceRequest(workspace));

            var locations = result.Features[typeof(ProgramDescriptor)].As<ProgramDescriptor>()
                .VariableLocations
                .Where(variable => variable.Name == "a")
                .SelectMany(variable => variable.Locations)
                .Select(location => location.StartLine);

            var expectedLocations = linePositionSpans["a"].Select(loc => loc.Start.Line);

            locations.Should().BeEquivalentTo(expectedLocations);
        }

        [Fact]
        public async Task When_Run_is_called_with_instrumentation_and_regions_lines_are_mapped()
        {
            var code = @"
using System;

namespace ConsoleProgram
{
    public class Program
    {
        public static void Main(string[] args)
        {
#region reg
#endregion
        }
    }
}";
            var regionCodeWithMarkup = @"
{|line:Console.WriteLine();|}
{|line:Console.WriteLine();|}";
            MarkupTestFile.GetNamedSpans(regionCodeWithMarkup, out var regionCode, out var spans);
            var linePositionSpans = ToLinePositionSpan(spans, regionCode);

            var workspace = new Workspace(
                workspaceType: "console",
                buffers: new[] { new Workspace.Buffer("test.cs@reg", regionCode)},
                files: new [] { new Workspace.File("test.cs", code)},
                includeInstrumentation: true
                );

            var server = await GetRunner();
            var result = await server.Run(new WorkspaceRequest(workspace));
            var filePositions = result.Features[typeof(ProgramStateAtPositionArray)].As<ProgramStateAtPositionArray>()
                .ProgramStates
                .Where(state => state.FilePosition != null)
                .Select(state => state.FilePosition.Line);

            var expectedLines = linePositionSpans["line"].Select(loc => loc.Start.Line);

            filePositions.Should().BeEquivalentTo(expectedLines);
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

            var result = await server.Run(new WorkspaceRequest(workspace));

            result.Succeeded.Should().BeTrue();
            result.Output.Count.Should().Be(21);
            result.Output.Should().BeEquivalentTo("1", "1", "2", "3", "5", "8", "13", "21", "34", "55", "89", "144", "233", "377", "610", "987", "1597", "2584", "4181", "6765", "");
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

            var result = await server.Run(new WorkspaceRequest(workspace));

            result.Succeeded.Should().BeTrue();
            result.Output.Count.Should().Be(21);
            result.Output.Should().BeEquivalentTo("1", "1", "2", "3", "5", "8", "13", "21", "34", "55", "89", "144", "233", "377", "610", "987", "1597", "2584", "4181", "6765", "");
        }

        private IDictionary<String, IEnumerable<LinePositionSpan>> ToLinePositionSpan(IDictionary<String, ImmutableArray<TextSpan>> input, string code)
            => input.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.Select(span => span.ToLinePositionSpan(SourceText.From(code))));

        protected override async Task<ICodeRunner> GetRunner(
            [CallerMemberName] string testName = null)
        {
            return new RoslynWorkspaceServer(WorkspaceRegistry.CreateDefault());
        }

        protected override ILanguageService GetLanguageService(
            [CallerMemberName] string testName = null) => new RoslynWorkspaceServer(
                WorkspaceRegistry.CreateDefault());

    }
}
