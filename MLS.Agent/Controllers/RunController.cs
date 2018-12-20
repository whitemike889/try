using System;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.AspNetCore.Mvc;
using MLS.Agent.Middleware;
using MLS.Protocol;
using MLS.Protocol.Execution;
using Pocket;
using WorkspaceServer;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Roslyn;
using WorkspaceServer.Servers.Scripting;
using WorkspaceServer.WorkspaceFeatures;
using static Pocket.Logger<MLS.Agent.Controllers.RunController>;

namespace MLS.Agent.Controllers
{
    public class RunController : Controller
    {
        private readonly StartupOptions _options;
        private readonly RoslynWorkspaceServer _workspaceServer;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public RunController(
            WorkspaceRegistry workspaceRegistry,
            StartupOptions options,
            RoslynWorkspaceServer workspaceServer)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _workspaceServer = workspaceServer;
        }

        [HttpPost]
        [Route("/workspace/run")]
        [DebugEnableFilter]
        public async Task<IActionResult> Run(
            [FromBody] WorkspaceRequest request,
            [FromHeader(Name = "Timeout")] string timeoutInMilliseconds = "15000")
        {
            if (_options.IsLanguageService)
            {
                return NotFound();
            }

            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                var workspaceType = request.Workspace.WorkspaceType;

                operation.Info("Processing workspaceType {workspaceType}", workspaceType);

                if (!int.TryParse(timeoutInMilliseconds, out var timeoutMs))
                {
                    return BadRequest();
                }

                RunResult result;
                var runTimeout = TimeSpan.FromMilliseconds(timeoutMs);

                var budget = new TimeBudget(runTimeout);

                if (string.Equals(workspaceType, "script", StringComparison.OrdinalIgnoreCase))
                {
                    var server = new ScriptingWorkspaceServer();

                    result = await server.Run(
                                 request,
                                 budget);
                }
                else
                {
                    using (result = await _workspaceServer.Run(request, budget))
                    {
                        _disposables.Add(result);

                        if (result.Succeeded &&
                            request.HttpRequest != null)
                        {
                            var webServer = result.GetFeature<WebServer>();

                            if (webServer != null)
                            {
                                var response = await webServer.SendAsync(
                                                                  request.HttpRequest.ToHttpRequestMessage())
                                                              .CancelIfExceeds(budget);

                                result = new RunResult(
                                    true,
                                    await response.ToDisplayString());
                            }
                        }
                    }
                }

                budget.RecordEntry();
                operation.Succeed();

                return Ok(result);
            }
        }
    }
}
