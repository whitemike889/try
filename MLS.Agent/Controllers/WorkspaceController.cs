using System.IO;
using System.Threading.Tasks;
using WorkspaceServer;
using Microsoft.AspNetCore.Mvc;

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
            var workingDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "workspaces", workspaceId));
            var server = new LocalWorkspaceServer(workingDirectory);

            var result = await server.CompileAndExecute(request);

            return Ok(result);
        }
    }
}
