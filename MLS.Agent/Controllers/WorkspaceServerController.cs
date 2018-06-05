using System;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.AspNetCore.Mvc;
using Pocket;
using WorkspaceServer;

namespace MLS.Agent.Controllers
{
    public class WorkspaceServerController : Controller
    {
        private readonly WorkspaceServerRegistry _workspaceServerRegistry;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public WorkspaceServerController(WorkspaceServerRegistry workspaceServerRegistry)
        {
            _workspaceServerRegistry = workspaceServerRegistry ?? throw new ArgumentNullException(nameof(workspaceServerRegistry));
        }

        protected  Task<IWorkspaceServer> GetWorkspaceServer(string workspaceType, Budget budget = null)
        {
            return _workspaceServerRegistry.GetWorkspaceServer(workspaceType, budget);
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
