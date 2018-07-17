using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Roslyn;
using Xunit;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public class RoslynWorkspaceServerConsoleProjectIntellisenseTests : WorkspaceServerTestsCore
    {
        public RoslynWorkspaceServerConsoleProjectIntellisenseTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task Get_autocompletion_for_console_class()
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

            var workspace = new Workspace(workspaceType: "console", buffers: new[]
            {
                new Workspace.Buffer("Program.cs", program),
                new Workspace.Buffer("generators/FibonacciGenerator.cs", processed, position)
            });

            var request = new WorkspaceRequest(workspace, activeBufferId: "generators/FibonacciGenerator.cs");
            var server = GetLanguageService();
            var result = await server.GetCompletionList(request);
            result.Items.Should().NotBeNullOrEmpty();
            result.Items.Should().NotContain(signature => string.IsNullOrEmpty(signature.Kind));
            result.Items.Should().Contain(completion => completion.SortText == "Console");
            var hasDuplicatedEntries = HasDuplicatedCompletionItems(result);
            hasDuplicatedEntries.Should().BeFalse();
        }

        [Fact]
        public async Task Get_autocompletion_for_console_methods()
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
                Console.$$
            }
        }
    }
}";

            #endregion

            var (processed, position) = CodeManipulation.ProcessMarkup(generator);

            var workspace = new Workspace(workspaceType: "console", buffers: new[]
            {
                new Workspace.Buffer("Program.cs", CodeManipulation.EnforceLF(program)),
                new Workspace.Buffer("generators/FibonacciGenerator.cs", processed, position)
            });

            var request = new WorkspaceRequest(workspace, activeBufferId: "generators/FibonacciGenerator.cs");
            var server = GetLanguageService();
            var result = await server.GetCompletionList(request);

            result.Items.Should().NotBeNullOrEmpty();
            result.Items.Should().NotContain(signature => string.IsNullOrEmpty(signature.Kind));
            result.Items.Should().Contain(completion => completion.SortText == "Beep");
            var hasDuplicatedEntries = HasDuplicatedCompletionItems(result);
            hasDuplicatedEntries.Should().BeFalse();

        }

        [Fact]
        public async Task Get_autocompletion_for_jtoken()
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

            var (processed, position) = CodeManipulation.ProcessMarkup("JTo$$;");

            var workspace = new Workspace(
                workspaceType: "console",
                buffers: new[] {
                    new Workspace.Buffer("Program.cs", CodeManipulation.EnforceLF(program)),
                    new Workspace.Buffer("generators/FibonacciGenerator.cs@codeRegion", processed, position)
                }, files: new[] {
                    new Workspace.File("generators/FibonacciGenerator.cs", CodeManipulation.EnforceLF(generator)),
                });

            var request = new WorkspaceRequest(workspace, activeBufferId: "generators/FibonacciGenerator.cs@codeRegion");
            var server = GetLanguageService();
            var result = await server.GetCompletionList(request);

            result.Items.Should().NotBeNullOrEmpty();
            result.Items.Should().NotContain(signature => string.IsNullOrEmpty(signature.Kind));
            result.Items.Should().Contain(signature => signature.SortText == "JToken");
            var hasDuplicatedEntries = HasDuplicatedCompletionItems(result);
            hasDuplicatedEntries.Should().BeFalse();
        }

        [Fact]
        public async Task Get_autocompletion_for_jtoken_methods()
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
}".EnforceLF();

            #endregion

            var (processed, position) = CodeManipulation.ProcessMarkup("JToken.fr$$;");

            var workspace = new Workspace(
                workspaceType: "console",
                buffers: new[] {
                    new Workspace.Buffer("Program.cs", program),
                    new Workspace.Buffer("generators/FibonacciGenerator.cs@codeRegion", processed, position)
                }, files: new[] {
                    new Workspace.File("generators/FibonacciGenerator.cs",generator),
                });

            var request = new WorkspaceRequest(workspace, activeBufferId: "generators/FibonacciGenerator.cs@codeRegion");
            var server = GetLanguageService();
            var result = await server.GetCompletionList(request);
            result.Items.Should().NotBeNullOrEmpty();
            result.Items.Should().NotContain(signature => string.IsNullOrEmpty(signature.Kind));
            result.Items.Should().Contain(signature => signature.SortText == "FromObject");
            var hasDuplicatedEntries = HasDuplicatedCompletionItems(result);
            hasDuplicatedEntries.Should().BeFalse();
        }

        [Fact]
        public async Task Get_autocompletion_can_be_empty()
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

            var generator = @"
#region codeRegion
#endregion
".EnforceLF();

            #endregion

            var (processed, position) = CodeManipulation.ProcessMarkup("class $$");

            var workspace = new Workspace(
                workspaceType: "console",
                buffers: new[] {
                    new Workspace.Buffer("Program.cs", program),
                    new Workspace.Buffer("generators/FibonacciGenerator.cs@codeRegion", processed, position)
                }, files: new[] {
                    new Workspace.File("generators/FibonacciGenerator.cs",generator),
                });

            var request = new WorkspaceRequest(workspace, activeBufferId: "generators/FibonacciGenerator.cs@codeRegion");
            var server = GetLanguageService();
            var result = await server.GetCompletionList(request);
            result.Items.Should().BeEmpty();
        }

        private static bool HasDuplicatedCompletionItems(CompletionResult result)
        {
            var g = result.Items.GroupBy(ci => ci.Kind + ci.InsertText).Select(cig => new { Key = cig.Key, Count = cig.Count() });
            var duplicatedEntries = g.Where(cig => cig.Count > 1);
            return duplicatedEntries.Any();
        }

        [Fact]
        public async Task Get_signature_help_for_console_writeline()
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

            var workspace = new Workspace(workspaceType: "console", buffers: new[]
            {
                new Workspace.Buffer("Program.cs", program),
                new Workspace.Buffer("generators/FibonacciGenerator.cs", processed, position)
            });

            var request = new WorkspaceRequest(workspace, activeBufferId: "generators/FibonacciGenerator.cs");
            var server = GetLanguageService();
            var result = await server.GetSignatureHelp(request);

            result.Signatures.Should().NotBeNullOrEmpty();
            result.Signatures.Should().Contain(signature => signature.Label == "void Console.WriteLine(string format, params object[] arg)");

        }

        [Fact]
        public async Task Get_signature_help_for_invalid_location_return_empty()
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
                Console.WriteLine();$$
            }
        }
    }
}".EnforceLF();

            #endregion

            var (processed, position) = CodeManipulation.ProcessMarkup(generator);

            var workspace = new Workspace(workspaceType: "console", buffers: new[]
            {
                new Workspace.Buffer("Program.cs", program),
                new Workspace.Buffer("generators/FibonacciGenerator.cs", processed, position)
            });

            var request = new WorkspaceRequest(workspace, activeBufferId: "generators/FibonacciGenerator.cs");
            var server = GetLanguageService();
            var result = await server.GetSignatureHelp(request);
            result.Should().NotBeNull();
            result.Signatures.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task Get_signature_help_for_console_writeline_with_region()
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
                #region codeRegion
                #endregion
            }
        }
    }
}".EnforceLF();

            #endregion

            var (processed, position) = CodeManipulation.ProcessMarkup("Console.WriteLine($$)");

            var workspace = new Workspace(
                workspaceType: "console",
                buffers: new[] {
                    new Workspace.Buffer("Program.cs", program),
                    new Workspace.Buffer("generators/FibonacciGenerator.cs@codeRegion", processed, position)
                }, files: new[] {
                    new Workspace.File("generators/FibonacciGenerator.cs", generator),
                });

            var request = new WorkspaceRequest(workspace, activeBufferId: "generators/FibonacciGenerator.cs@codeRegion");
            var server = GetLanguageService();
            var result = await server.GetSignatureHelp(request);

            result.Signatures.Should().NotBeNullOrEmpty();
            result.Signatures.Should().Contain(signature => signature.Label == "void Console.WriteLine(string format, params object[] arg)");

        }

        [Fact]
        public async Task Get_signature_help_for_jtoken()
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
}".EnforceLF();

            #endregion

            var (processed, position) = CodeManipulation.ProcessMarkup("JToken.FromObject($$);");

            var workspace = new Workspace(
                workspaceType: "console",
                buffers: new[] {
                    new Workspace.Buffer("Program.cs", program),
                    new Workspace.Buffer("generators/FibonacciGenerator.cs@codeRegion", processed, position)
                }, files: new[] {
                    new Workspace.File("generators/FibonacciGenerator.cs", generator),
                });

            var request = new WorkspaceRequest(workspace, activeBufferId: "generators/FibonacciGenerator.cs@codeRegion");
            var server = GetLanguageService();
            var result = await server.GetSignatureHelp(request);

            result.Signatures.Should().NotBeNullOrEmpty();
            result.Signatures.Should().Contain(signature => signature.Label == "JToken JToken.FromObject(object o)");

        }

        protected override Task<ICodeRunner> GetRunner(
            [CallerMemberName] string testName = null)
        {
            throw new NotImplementedException();
        }

        protected override ILanguageService GetLanguageService([CallerMemberName] string testName = null)
        {
            return new RoslynWorkspaceServer(WorkspaceRegistry.CreateDefault());
        }
    }
}
