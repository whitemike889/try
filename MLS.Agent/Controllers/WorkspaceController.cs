using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WorkspaceServer;

namespace MLS.Agent.Controllers
{
    public class WorkspaceController : Controller
    {
        [HttpPost]
        [Route("/workspace/{workspaceId}/compile")]
        public async Task<IActionResult> Run(
            string workspaceId,
            [FromBody] BuildAndRunRequest request)
        {
            var server = new ScriptingWorkspaceServer();

            var result = await server.CompileAndExecute(request);

            return Ok(result);
        }
    }
}