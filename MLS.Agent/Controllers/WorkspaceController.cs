using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pocket;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Scripting;
using static Pocket.Logger<MLS.Agent.Controllers.WorkspaceController>;

namespace MLS.Agent.Controllers
{
    public class WorkspaceController : Controller
    {
        [HttpPost]
        [Route("/workspace/{workspaceId}/compile")]
        public async Task<IActionResult> Run(
            string workspaceId,
            [FromBody] RunRequest request)
        {
            using (var operation = Log.ConfirmOnExit())
            {
                var server = new ScriptingWorkspaceServer();

                var result = await server.Run(request);

                operation.Succeed();

                return Ok(result);
            }
        }

        [HttpPost]
        [Route("/workspace/{workspaceId}/getCompletionItems")]
        public async Task<IActionResult> GetCompletionItems(
            string workspaceId,
            [FromBody] CompletionRequest request)
        {
            using (var operation = Log.ConfirmOnExit())
            {
                var server = new ScriptingWorkspaceServer();

                var result = await server.GetCompletionList(request);

                operation.Succeed();

                return Ok(result);
            }
        }
    }
}
