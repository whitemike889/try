using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MLS.TestSupport;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Scripting;
using Xunit;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public class ScriptingWorkspaceServerIntellisenseTests : WorkspaceServerTestsCore
    {
        public ScriptingWorkspaceServerIntellisenseTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override ILanguageService GetLanguageService(string testName = null) =>
            new ScriptingWorkspaceServer();

        protected override Task<ICodeRunner> GetRunner(string testName = null)
        {
            return Task.FromResult<ICodeRunner>(new ScriptingWorkspaceServer());
        }

        [Fact]
        public async Task Get_signature_help_for_invalid_location_return_empty()
        {
            var code = @"using System;
using System.Collections.Generic;
using System.Linq;

public class Program
{
  public static void Main()
  {
    foreach (var i in Fibonacci().Take())$$
    {
      Console.WriteLine(i);
    }
  }

  private static IEnumerable<int> Fibonacci()
  {
    int current = 1, next = 1;

    while (true)
    {
      yield return current;
      next = current + (current = next);
    }
  }
}";
            var (processed, markLocation) = CodeManipulation.ProcessMarkup(code);

            var ws = new Workspace(buffers: new[] { new Workspace.Buffer("", processed, 0) });
            var request = new WorkspaceRequest(ws, activeBufferId: "", position: markLocation);
            var server = GetLanguageService();
            var result = await server.GetSignatureHelp(request);
            result.Should().NotBeNull();
            result.Signatures.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task Can_show_signature_help_for_extensions()
        {
            var code = @"using System;
using System.Collections.Generic;
using System.Linq;

public class Program
{
  public static void Main()
  {
    foreach (var i in Fibonacci().Take($$))
    {
      Console.WriteLine(i);
    }
  }

  private static IEnumerable<int> Fibonacci()
  {
    int current = 1, next = 1;

    while (true)
    {
      yield return current;
      next = current + (current = next);
    }
  }
}";
            var (processed, markLocation) = CodeManipulation.ProcessMarkup(code);

            var ws = new Workspace(buffers: new[] { new Workspace.Buffer("", processed, 0) });
            var request = new WorkspaceRequest(ws, activeBufferId: "", position: markLocation);
            var server = GetLanguageService();
            var result = await server.GetSignatureHelp(request);
            result.Signatures.Should().NotBeEmpty();
            result.Signatures.First().Label.Should().Be("IEnumerable<TSource> Enumerable.Take<TSource>(IEnumerable<TSource> source, int count)");
        }

        [Fact]
        public async Task Can_show_KeyValuePair_because_it_uses_the_right_reference_assemblies()
        {
            var (processed, markLocation) = CodeManipulation.ProcessMarkup("System.Collections.Generic.$$");

            var ws = new Workspace(buffers: new[] { new Workspace.Buffer("default.cs", processed, 0) });
            var request = new WorkspaceRequest(ws, activeBufferId: "default.cs", position: markLocation);
            var server = GetLanguageService();
            var result = await server.GetCompletionList(request);

            result.Items.Should().NotBeNullOrEmpty();
            result.Items.Should().Contain(i => i.DisplayText == "KeyValuePair");
        }

        [Fact]
        public async Task Can_show_completions()
        {
            var (processed, markLocation) = CodeManipulation.ProcessMarkup("var xa = 3;\n$$a");
            var ws = new Workspace(buffers: new[] { new Workspace.Buffer("default.cs", processed, 0) });
            var request = new WorkspaceRequest(ws, activeBufferId: "default.cs", position: markLocation);
            var server = GetLanguageService();
            var result = await server.GetCompletionList(request);

            result.Items.Should().NotBeNullOrEmpty();
            result.Items.Should().Contain(i => i.DisplayText == "xa");
        }

        [Fact]
        public async Task Can_get_signatureHelp_for_workspace_with_buffers()
        {
            var container = @"class A
{
    #region nesting
    #endregion
    void Operation()
    {
        var instance = new C();
    }
}";
            var markup = @"class C
{
    public void Foo() { Foo($$ }
}";

            var (processed, markLocation) = CodeManipulation.ProcessMarkup(markup);

            var ws = new Workspace(
                files: new[] { new Workspace.File("program.cs", CodeManipulation.EnforceLF(container)) },
                buffers: new[] { new Workspace.Buffer("program.cs@nesting", processed, 0) });


            var request = new WorkspaceRequest(ws, activeBufferId: "program.cs@nesting", position: markLocation);
            var server = GetLanguageService();
            var result = await server.GetSignatureHelp(request);

            result.Signatures.Should().NotBeEmpty();
            result.Signatures.First().Label.Should().Be("void C.Foo()");
        }

        [Fact]
        public async Task Can_show_signatureHelp_for_workspace()
        {
            var markup = @"class C
{
    void Foo() { Foo($$ }
}";

            var (processed, markLocation) = CodeManipulation.ProcessMarkup(markup);
            var ws = new Workspace(buffers: new[] { new Workspace.Buffer("program.cs", processed, 0) });

            var request = new WorkspaceRequest(ws, activeBufferId: "program.cs", position: markLocation);
            var server = GetLanguageService();
            var result = await server.GetSignatureHelp(request);
            result.Signatures.Should().NotBeEmpty();
            result.Signatures.First().Label.Should().Be("void C.Foo()");
        }

        [Fact]
        public async Task Can_show_all_completion_properties_for_Class_Task()
        {
            var ws = new Workspace(buffers: new[] { new Workspace.Buffer("default.cs", "System.Threading.Tasks.", 0) });
            var request = new WorkspaceRequest(ws, activeBufferId: "default.cs", position: 23);
            var server = GetLanguageService();
            var result = await server.GetCompletionList(request);
            var taskCompletionItem = result.Items.First(i => i.DisplayText == "Task");

            taskCompletionItem.DisplayText.Should().Be("Task");
            taskCompletionItem.FilterText.Should().Be("Task");
            taskCompletionItem.Kind.Should().Be("Class");
            taskCompletionItem.SortText.Should().Be("Task");
        }
    }
}
