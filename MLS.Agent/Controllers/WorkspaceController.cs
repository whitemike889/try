using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Scripting;

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
            var server = new ScriptingWorkspaceServer();

            var result = await server.CompileAndExecute(request);

            return Ok(result);
        }

        [HttpPost]
        [Route("/workspace/{workspaceId}/getCompletionItems")]
        public async Task<IActionResult> Run(
            string workspaceId,
            [FromBody] CompletionRequest request)
        {
            var server = new ScriptingWorkspaceServer();

            var result = await server.GetCompletionList(request);

            return Ok(result);
        }
    }
}
