using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.AspNetCore.Mvc;
using MLS.Protocol;
using MLS.Protocol.Execution;
using Pocket;
using WorkspaceServer;
using WorkspaceServer.Servers.Roslyn;
using static Pocket.Logger<MLS.Agent.Controllers.CompileController>;

namespace MLS.Agent.Controllers
{
    public class CompileController : Controller
    {
        private readonly AgentOptions _options;
        private readonly RoslynWorkspaceServer _workspaceServer;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public CompileController(
            WorkspaceRegistry workspaceRegistry,
            RoslynWorkspaceServer imws,
            AgentOptions options,
            RoslynWorkspaceServer workspaceServer)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _workspaceServer = workspaceServer;
        }

        protected Task<ICodeCompiler> GetWorkspaceServer(string workspaceType, Budget budget = null)
        {
            return Task.FromResult(_workspaceServer as ICodeCompiler);
        }

        [HttpPost]
        [Route("/workspace/compile")]
        public async Task<IActionResult> Compile(
            [FromBody] WorkspaceRequest request,
            [FromHeader(Name = "Timeout")] string timeoutInMilliseconds = "15000")
        {
            if (_options.IsLanguageServiceMode)
            {
                return StatusCode(404);
            }

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

                CompileResult result;
                var workspaceType = request.Workspace.WorkspaceType;
                var runTimeout = TimeSpan.FromMilliseconds(timeoutMs);

                var budget = new TimeBudget(runTimeout);

                var server = await GetWorkspaceServer(workspaceType);
                if (server == null)
                {
                    return BadRequest();
                }

                result = await server.Compile(request, budget);
                budget?.RecordEntry();
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
