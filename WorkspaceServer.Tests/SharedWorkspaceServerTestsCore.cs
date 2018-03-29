using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WorkspaceServer.Servers.Dotnet;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public abstract class SharedWorkspaceServerTestsCore : WorkspaceServerTestsCore
    {
        private static IWorkspaceServer _sharedServer;
        private static RefCountDisposable _sharedDisposable;
        private readonly SerialDisposable _finaliser = new SerialDisposable();

        protected SharedWorkspaceServerTestsCore(ITestOutputHelper output) : base(output)
        {
            
        }

        protected  async Task<IWorkspaceServer> GetSharedWorkspaceServer(
            [CallerFilePath] string callerPath = null)
        {
            if (_sharedServer != null)
            {
                RegisterForDisposal(_sharedDisposable.GetDisposable());
                RegisterForDelayedDisposal();
                return _sharedServer;
            }
            var testName = Path.GetFileNameWithoutExtension(callerPath);
            var project = await Create.ConsoleWorkspace(testName);
            var workspaceServer = new DotnetWorkspaceServer(project, 45);
            
            await workspaceServer.EnsureInitializedAndNotDisposed();
            _sharedServer = workspaceServer;
            _sharedDisposable = new RefCountDisposable(workspaceServer);
            
            RegisterForDisposal(_sharedDisposable.GetDisposable());
            RegisterForDelayedDisposal();
            return _sharedServer;
        }

        private void RegisterForDelayedDisposal()
        {
            RegisterForDisposal(Disposable.Create(() =>
            {
                _finaliser.Disposable = TaskPoolScheduler.Default.Schedule(
                    _sharedDisposable, 
                    TimeSpan.FromMinutes(5), (disposable, _) =>
                    {
                        _sharedServer = null;
                        _sharedDisposable = null;
                        disposable?.Dispose();
                    }
                );
            }));
        }
    }
}