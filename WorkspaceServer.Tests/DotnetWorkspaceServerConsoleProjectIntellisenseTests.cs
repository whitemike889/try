using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Clockwise;
using FluentAssertions;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Models.SingatureHelp;
using WorkspaceServer.Servers.Dotnet;
using Xunit;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public class DotnetWorkspaceServerConsoleProjectIntellisenseTests : WorkspaceServerTests
    {
        public DotnetWorkspaceServerConsoleProjectIntellisenseTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task Get_autocompletion_for_console()
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

            var workspace = new Workspace(workspaceType: "console", buffers: new[]
            {
                new Workspace.Buffer("Program.cs",program,0),
                new Workspace.Buffer("generators/FibonacciGenerator.cs",generator,0)
            });

            var position = generator.IndexOf(consoleWriteline, StringComparison.Ordinal) + consoleWriteline.Length;
            var request = new CompletionRequest(workspace, position: position, activeBufferId: "generators/FibonacciGenerator.cs");

            using (var clock = VirtualClock.Start())
            {
                var server = await GetWorkspaceServer();
                var result = await server.GetCompletionList(request);

                result.Items.Should().NotBeNullOrEmpty();
                result.Items.Should().Contain(completion => completion.SortText == "void Console.WriteLine(string format, params object[] arg)");
            }
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

            var workspace = new Workspace(
                workspaceType: "console",
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

                var server = await GetWorkspaceServer();
                var run = await server.Run(workspace, new Budget());
                var result = await server.GetSignatureHelp(request);

                result.Signatures.Should().NotBeNullOrEmpty();
                result.Signatures.Should().Contain(signature => signature.Label == "JToken JToken.FromObject(object o)");
            }
        }

        protected override Workspace CreateWorkspaceContaining(string text)
        {
            return new Workspace(
                $@"using System; using System.Linq; using System.Collections.Generic; class Program {{ static void Main() {{ {text}
                    }}
                }}
");
        }

        protected override async Task<IWorkspaceServer> GetWorkspaceServer(
            [CallerMemberName] string testName = null)
        {
            var project = await Create.ConsoleWorkspace(testName);

            var workspaceServer = new DotnetWorkspaceServer(project, 45);

            RegisterForDisposal(workspaceServer);

            await workspaceServer.EnsureInitializedAndNotDisposed();

            return workspaceServer;
        }
    }
}