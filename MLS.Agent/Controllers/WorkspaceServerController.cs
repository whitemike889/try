using System;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.AspNetCore.Mvc;
using Pocket;
using WorkspaceServer;
using WorkspaceServer.Servers.InMemory;

namespace MLS.Agent.Controllers
{
    public class RunController : Controller
    {
        private readonly RoslynWorkspaceServer _workspaceServer;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public RunController(RoslynWorkspaceServer workspaceServer)
        {
            _workspaceServer = workspaceServer ?? throw new ArgumentNullException(nameof(workspaceServer));
        }

        protected  Task<ICodeRunner> GetWorkspaceServer(string workspaceType, Budget budget = null)
        {
            return Task.FromResult((ICodeRunner)_workspaceServer);
        }

        protected void AddToDisposeChain(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposables.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
