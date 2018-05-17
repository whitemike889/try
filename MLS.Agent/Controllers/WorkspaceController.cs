using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.AspNetCore.Mvc;
using Pocket;
using Recipes;
using WorkspaceServer;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Dotnet;
using WorkspaceServer.Servers.Scripting;
using WorkspaceServer.WorkspaceFeatures;
using static Pocket.Logger<MLS.Agent.Controllers.WorkspaceController>;

namespace MLS.Agent.Controllers
{
    public class WorkspaceController : Controller
    {
        private readonly WorkspaceServerRegistry _workspaceServerRegistry;
        private readonly AgentOptions _options;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public WorkspaceController(WorkspaceServerRegistry workspaceServerRegistry, AgentOptions options)
        {
            _workspaceServerRegistry = workspaceServerRegistry ?? throw new ArgumentNullException(nameof(workspaceServerRegistry));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        [HttpPost]
        [Route("/workspace/run")]
        public async Task<IActionResult> Run(
            [FromBody] WorkspaceRequest request,
            [FromHeader(Name = "Referer")] string referer,
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

            using (var operation = Log.OnEnterAndConfirmOnExit(
                properties: new object[] { ("workspaceType", request.Workspace.WorkspaceType) }))
            {
                if (!int.TryParse(timeoutInMilliseconds, out var timeoutMs))
                {
                    return BadRequest();
                }

                RunResult result;
                var workspaceType = request.Workspace.WorkspaceType;
                var runTimeout = TimeSpan.FromMilliseconds(timeoutMs);

                var budget = new TimeBudget(runTimeout);

                if (string.Equals(workspaceType, "script", StringComparison.OrdinalIgnoreCase))
                {
                    var server = new ScriptingWorkspaceServer();

                    result = await server.Run(
                                 request.Workspace,
                                 budget);
                }
                else
                {
                    var server = await _workspaceServerRegistry.GetWorkspaceServer(workspaceType);

                    if (server is DotnetWorkspaceServer dotnetWorkspaceServer)
                    {
                        await dotnetWorkspaceServer.EnsureInitializedAndNotDisposed(budget);
                    }

                    using (result = await server.Run(request.Workspace, budget))
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
