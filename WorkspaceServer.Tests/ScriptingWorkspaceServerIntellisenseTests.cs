using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
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

        protected override Task<IWorkspaceServer> GetWorkspaceServer(string testName = null)
        {
            return Task.FromResult<IWorkspaceServer>(new ScriptingWorkspaceServer());
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
    foreach (var i in Fibonacci().Take())
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

            var toFind = @"    foreach (var i in Fibonacci().Take(";
            var position = code.IndexOf(toFind) + toFind.Length;
            var ws = new Workspace(buffers: new[] { new Workspace.Buffer("", code, 0) });
            var request = new WorkspaceRequest(ws, activeBufferId: "", position: position);
            var server = await GetWorkspaceServer();
            var result = await server.GetSignatureHelp(request);
            result.Signatures.Should().NotBeEmpty();
            result.Signatures.First().Label.Should().Be("IEnumerable<TSource> Enumerable.Take<TSource>(IEnumerable<TSource> source, int count)");
        }

        [Fact]
        public async Task Can_show_KeyValuePair_because_it_uses_the_right_reference_assemblies()
        {
            var ws = new Workspace(buffers: new[] { new Workspace.Buffer("default.cs", "System.Collections.Generic.", 0) });
            var request = new WorkspaceRequest(ws, activeBufferId: "default.cs", position: 27);
            var server = await GetWorkspaceServer();
            var result = await server.GetCompletionList(request);

            result.Items.Should().NotBeNullOrEmpty();
            result.Items.Should().Contain(i => i.DisplayText == "KeyValuePair");
        }

        [Fact]
        public async Task Can_show_completions()
        {
            var ws = new Workspace(buffers: new[] { new Workspace.Buffer("default.cs", "var xa = 3;\na", 0) });
            var request = new WorkspaceRequest(ws, activeBufferId: "default.cs", position: 13);
            var server = await GetWorkspaceServer();
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

            var source = markup.Replace("$$", string.Empty);
            var position = markup.IndexOf("$$");
            var ws = new Workspace(
                files: new[] { new Workspace.File("program.cs", container) },
                buffers: new[] { new Workspace.Buffer("program.cs@nesting", source, 0) });
           
          
            var request = new WorkspaceRequest(ws, activeBufferId: "program.cs@nesting", position: position);
            var server = await GetWorkspaceServer();
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

            var source = markup.Replace("$$", string.Empty);
            var position = markup.IndexOf("$$");
            var ws = new Workspace(buffers: new[] { new Workspace.Buffer("program.cs", source, 0) });

            var request = new WorkspaceRequest(ws, activeBufferId: "program.cs", position: position);
            var server = await GetWorkspaceServer();
            var result = await server.GetSignatureHelp(request);
            result.Signatures.Should().NotBeEmpty();
            result.Signatures.First().Label.Should().Be("void C.Foo()");
        }

        [Fact]
        public async Task Can_show_all_completion_properties_for_Class_Task()
        {
            var ws = new Workspace(buffers: new[] { new Workspace.Buffer("default.cs", "System.Threading.Tasks.", 0) });
            var request = new WorkspaceRequest(ws, activeBufferId: "default.cs", position: 23);
            var server = await GetWorkspaceServer();
            var result = await server.GetCompletionList(request);
            var taskCompletionItem = result.Items.First(i => i.DisplayText == "Task");

            taskCompletionItem.DisplayText.Should().Be("Task");
            taskCompletionItem.FilterText.Should().Be("Task");
            taskCompletionItem.Kind.Should().Be("Class");
            taskCompletionItem.SortText.Should().Be("Task");
        }
    }
}