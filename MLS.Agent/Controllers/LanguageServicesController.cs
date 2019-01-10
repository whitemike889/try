using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.AspNetCore.Mvc;
using MLS.Agent.Middleware;
using MLS.Protocol;
using Pocket;
using WorkspaceServer;
using WorkspaceServer.Servers.Roslyn;
using WorkspaceServer.Servers.Scripting;
using static Pocket.Logger<MLS.Agent.Controllers.LanguageServicesController>;
using Workspace = MLS.Protocol.Execution.Workspace;

namespace MLS.Agent.Controllers
{
    public class LanguageServicesController : Controller
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly RoslynWorkspaceServer workspaceServer;

        public LanguageServicesController(PackageRegistry packageRegistry, RoslynWorkspaceServer workspaceServer)
        {
            this.workspaceServer = workspaceServer ?? throw new ArgumentNullException(nameof(workspaceServer));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposables.Dispose();
            }

            base.Dispose(disposing);
        }

        [HttpPost]
        [Route("/workspace/completion")]
        [DebugEnableFilter]
        public async Task<IActionResult> Completion(
            [FromBody] WorkspaceRequest request,
            [FromHeader(Name = "Timeout")] string timeoutInMilliseconds = "15000")
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                operation.Info("Processing workspaceType {workspaceType}", request.Workspace.WorkspaceType);
                if (!int.TryParse(timeoutInMilliseconds, out var timeoutMs))
                {
                    return BadRequest();
                }

                var runTimeout = TimeSpan.FromMilliseconds(timeoutMs);
                var budget = new TimeBudget(runTimeout);
                var server = GetServerForWorkspace(request.Workspace);
                var result = await server.GetCompletionList(request, budget);
                budget.RecordEntry();
                operation.Succeed();

                return Ok(result);
            }
        }
    
        [HttpPost]
        [Route("/workspace/signaturehelp")]
        public async Task<IActionResult> SignatureHelp(
            [FromBody] WorkspaceRequest request,
            [FromHeader(Name = "Timeout")] string timeoutInMilliseconds = "15000")
        {
            if (Debugger.IsAttached && !(Clock.Current is VirtualClock))
            {
                _disposables.Add(VirtualClock.Start());
            }

            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                operation.Info("Processing workspaceType {workspaceType}", request.Workspace.WorkspaceType);
                if (!int.TryParse(timeoutInMilliseconds, out var timeoutMs))
                {
                    return BadRequest();
                }

                var runTimeout = TimeSpan.FromMilliseconds(timeoutMs);
                var budget = new TimeBudget(runTimeout);
                var server = GetServerForWorkspace(request.Workspace);
                var result = await server.GetSignatureHelp(request, budget);
                budget.RecordEntry();
                operation.Succeed();

                return Ok(result);
            }
        }

        [HttpPost]
        [Route("/workspace/diagnostics")]
        public async Task<IActionResult> Diagnostics(
            [FromBody] WorkspaceRequest request,
            [FromHeader(Name = "Timeout")] string timeoutInMilliseconds = "15000")
        {
            if (Debugger.IsAttached && !(Clock.Current is VirtualClock))
            {
                _disposables.Add(VirtualClock.Start());
            }

            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                operation.Info("Processing workspaceType {workspaceType}", request.Workspace.WorkspaceType);
                if (!int.TryParse(timeoutInMilliseconds, out var timeoutMs))
                {
                    return BadRequest();
                }

                var runTimeout = TimeSpan.FromMilliseconds(timeoutMs);
                var budget = new TimeBudget(runTimeout);
                var server = GetServerForWorkspace(request.Workspace);
                var result = await server.GetDiagnostics(request, budget);
                budget.RecordEntry();
                operation.Succeed();

                return Ok(result);
            }
        }

        private ILanguageService GetServerForWorkspace(Workspace workspace)
        {
            if (string.Equals(workspace.WorkspaceType, "script", StringComparison.OrdinalIgnoreCase))
            {
                return new ScriptingWorkspaceServer();
            }
            else
            {
                return workspaceServer;
            }
        }
    }
}
