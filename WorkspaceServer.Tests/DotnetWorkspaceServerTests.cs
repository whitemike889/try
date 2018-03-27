using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Clockwise;
using FluentAssertions;
using MLS.Agent.Tools;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Models.SingatureHelp;
using WorkspaceServer.Servers.Dotnet;
using Xunit;
using Xunit.Abstractions;
using Workspace = WorkspaceServer.Models.Execution.Workspace;

namespace WorkspaceServer.Tests
{
    public class DotnetWorkspaceServerTests : WorkspaceServerTests
    {
        public DotnetWorkspaceServerTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override WorkspaceRequest CreateRunRequestContaining(string text)
        {
            var workspace = new Workspace(
                $@"using System; using System.Linq; using System.Collections.Generic; class Program {{ static void Main() {{ {text}
                    }}
                }}
");
            return new WorkspaceRequest(workspace);
        }

        [Fact]
        public async Task When_compile_is_unsuccessful_diagnostic_are_aligned_with_buffer_span()
        {
            var workspace = new Workspace(
                workspaceType: "console",
                files: new[] { new Workspace.File("Program.cs", CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion) },
                buffers: new[] { new Workspace.Buffer("Program.cs@alpha", @"Console.WriteLine(banana);", 0) });
            var request = new WorkspaceRequest(workspace);
            var server = await GetWorkspaceServer();

            var result = await server.Run(request);

            result.ShouldBeEquivalentTo(new
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
            var request = new WorkspaceRequest(workspace);
            var server = await GetWorkspaceServer();

            var result = await server.Run(request);

            result.ShouldBeEquivalentTo(new
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
            var server = await GetWorkspaceServer();

            var result = await server.GetDiagnostics(request.Workspace);

            result.Diagnostics.Should().Contain(d => d.Id == "CS0103" && d.Start == erroPos); // banana is not defined
        }

        [Fact]
        public async Task Workspace_cleans_previous_files()
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
            var request = new WorkspaceRequest(workspace);
            var server = await GetWorkspaceServer();

            var result = await server.Run(request);

            result.Succeeded.Should().BeTrue();

            workspace = new Workspace(workspaceType: "console", buffers: new[]
            {
                new Workspace.Buffer("NotProgram.cs", program, 0),
                new Workspace.Buffer("FibonacciGenerator.cs", generator, 0)
            });
            request = new WorkspaceRequest(workspace);
            result = await server.Run(request);

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
            var request = new WorkspaceRequest(workspace);
            var server = await GetWorkspaceServer();

            var result = await server.Run(request);

            result.Succeeded.Should().BeTrue();
            result.Output.Count.Should().Be(20);
            result.Output.ShouldAllBeEquivalentTo(new[]{
                "1",
                "1",
                "2",
                "3",
                "5",
                "8",
                "13",
                "21",
                "34",
                "55",
                "89",
                "144",
                "233",
                "377",
                "610",
                "987",
                "1597",
                "2584",
                "4181",
                "6765"});
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
            var request = new WorkspaceRequest(workspace);
            var server = await GetWorkspaceServer();

            var result = await server.Run(request);

            result.Succeeded.Should().BeTrue();
            result.Output.Count.Should().Be(20);
            result.Output.ShouldAllBeEquivalentTo(new[]{
                "1",
                "1",
                "2",
                "3",
                "5",
                "8",
                "13",
                "21",
                "34",
                "55",
                "89",
                "144",
                "233",
                "377",
                "610",
                "987",
                "1597",
                "2584",
                "4181",
                "6765"});
        }

        [Fact]
        public async Task Get_signature_help_for_console_writeline()
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

            var workspace = new Workspace(workspaceType: "console", buffers: new[]
            {
                new Workspace.Buffer("Program.cs",program,0),
                new Workspace.Buffer("generators/FibonacciGenerator.cs",generator,0)
            });

            var position = generator.IndexOf(consoleWriteline, StringComparison.Ordinal) + consoleWriteline.Length;
            var request = new SignatureHelpRequest(workspace, position: position, activeBufferId: "generators/FibonacciGenerator.cs");

            using (var clock = VirtualClock.Start())
            {
                var server = await GetWorkspaceServer();
                var result = await server.GetSignatureHelp(request);

                result.Signatures.Should().NotBeNullOrEmpty();
                result.Signatures.Should().Contain(signature => signature.Label == "void Console.WriteLine(string format, params object[] arg)");
            }
        }

        [Fact]
        public async Task Get_signature_help_for_console_writeline_with_region()
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
                #region codeRegion
                #endregion
            }
        }
    }
}";

            #endregion

            var workspace = new Workspace(
                workspaceType: "console",
                buffers: new[] {
                    new Workspace.Buffer("Program.cs",program,0),
                    new Workspace.Buffer("generators/FibonacciGenerator.cs@codeRegion","Console.WriteLine()",0)
                }, files: new[] {
                    new Workspace.File("generators/FibonacciGenerator.cs",generator),
                });


            var position = 18;

            var request = new SignatureHelpRequest(workspace, position: position, activeBufferId: "generators/FibonacciGenerator.cs@codeRegion");

            using (var clock = VirtualClock.Start())
            {
                var server = await GetWorkspaceServer();
                var result = await server.GetSignatureHelp(request);

                result.Signatures.Should().NotBeNullOrEmpty();
                result.Signatures.Should().Contain(signature => signature.Label == "void Console.WriteLine(string format, params object[] arg)");
            }
        }

        [Fact]
        public async Task Get_signature_help_for_jtoken()
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                #region codeRegion
                #endregion
            }
        }
    }
}";

            #endregion

            var workspaceType = GetWorkspaceType();
            var workspace = new Workspace(
                workspaceType: workspaceType,
                buffers: new[] {
                    new Workspace.Buffer("Program.cs",program,0),
                    new Workspace.Buffer("generators/FibonacciGenerator.cs@codeRegion","JToken.FromObject();",0)
                }, files: new[] {
                    new Workspace.File("generators/FibonacciGenerator.cs",generator),
                });

            var position = 18;

            var request = new SignatureHelpRequest(workspace, position: position, activeBufferId: "generators/FibonacciGenerator.cs@codeRegion");

            using (var clock = VirtualClock.Start())
            {
                var wb = new WorkspaceBuilder(workspaceType);
                wb.CreateUsingDotnet("console");
                wb.AddPackageReference("Newtonsoft.Json");

                var project = await wb.GetWorkspace();

                var server = await GetWorkspaceServer(project);
                var result = await server.GetSignatureHelp(request);

                result.Signatures.Should().NotBeNullOrEmpty();
                result.Signatures.Should().Contain(signature => signature.Label == "JToken JToken.FromObject(object o)");
            }
        }

        private static string GetWorkspaceType([CallerMemberName] string testName = null)
        {
            return testName;
        }

        protected override async Task<IWorkspaceServer> GetWorkspaceServer(
            [CallerMemberName] string testName = null)
        {
            var project = await Create.TestWorkspace(testName);

            var workspaceServer = new DotnetWorkspaceServer(project, 45);

            RegisterForDisposal(workspaceServer);

            await workspaceServer.EnsureInitializedAndNotDisposed();

            return workspaceServer;
        }

        private async Task<IWorkspaceServer> GetWorkspaceServer(MLS.Agent.Tools.Workspace project)
        {
            var workspaceServer = new DotnetWorkspaceServer(project, 45);

            RegisterForDisposal(workspaceServer);

            await workspaceServer.EnsureInitializedAndNotDisposed();

            return workspaceServer;
        }
    }
}

