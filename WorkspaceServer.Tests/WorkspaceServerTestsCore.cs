using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Pocket;
using WorkspaceServer.Workspaces;
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

        protected abstract Task<(ICodeRunner runner, WorkspaceBuild workspace)> GetRunnerAndWorkspaceBuild(
            [CallerMemberName] string testName = null);

        protected abstract ILanguageService GetLanguageService(
            [CallerMemberName] string testName = null);
    }
}
