using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using MLS.Protocol;
using MLS.Protocol.Execution;
using Pocket;
using WorkspaceServer.Packaging;
using Xunit;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public abstract class WorkspaceServerTestsCore : IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        protected WorkspaceServerTestsCore(ITestOutputHelper output)
        {
            _disposables.Add(output.SubscribeToPocketLogger());
        }

        public void Dispose() => _disposables.Dispose();

        protected void RegisterForDisposal(IDisposable disposable) => _disposables.Add(disposable);

        protected abstract Task<(ICodeRunner runner, Package workspace)> GetRunnerAndWorkspaceBuild(
            [CallerMemberName] string testName = null);

        protected abstract ILanguageService GetLanguageService(
            [CallerMemberName] string testName = null);

        [Fact]
        public async Task Can_show_signatureHelp_for_script_code()
        {
            var markup = @"System.Console.WriteLine($$ ";

            var (processed, markLocation) = CodeManipulation.ProcessMarkup(markup);
            var ws = new Workspace(buffers: new[] { new Workspace.Buffer("program.cs", processed, markLocation) });

            var request = new WorkspaceRequest(ws, activeBufferId: "program.cs");
            var server = GetLanguageService();
            var result = await server.GetSignatureHelp(request);
            result.Signatures.Should().NotBeEmpty();
            result.Signatures.First().Label.Should().Be("void Console.WriteLine()");
        }
    }
}
