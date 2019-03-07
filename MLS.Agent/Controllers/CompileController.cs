using System;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.AspNetCore.Mvc;
using MLS.Agent.Middleware;
using MLS.Protocol;
using Pocket;
using WorkspaceServer.Servers.Roslyn;
using static Pocket.Logger<MLS.Agent.Controllers.CompileController>;

namespace MLS.Agent.Controllers
{
    public class CompileController : Controller
    {
        private readonly RoslynWorkspaceServer _workspaceServer;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public CompileController(
            RoslynWorkspaceServer workspaceServer)
        {
            _workspaceServer = workspaceServer;
        }

        [HttpPost]
        [Route("/workspace/compile")]
        [DebugEnableFilter]
        public async Task<IActionResult> Compile(
            [FromBody] WorkspaceRequest request,
            [FromHeader(Name = "Timeout")] string timeoutInMilliseconds = "45000")
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                var workspaceType = request.Workspace.WorkspaceType;

                operation.Info("Compiling workspaceType {workspaceType}", workspaceType);

                if (!int.TryParse(timeoutInMilliseconds, out var timeoutMs))
                {
                    return BadRequest();
                }

                if (string.Equals(workspaceType, "script", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest();
                }

                var runTimeout = TimeSpan.FromMilliseconds(timeoutMs);
                var budget = new TimeBudget(runTimeout);

                var result = await _workspaceServer.Compile(request, budget);
                budget.RecordEntry();
                operation.Succeed();
                return Ok(result);
            }
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
