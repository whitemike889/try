using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pocket;
using WorkspaceServer;
using WorkspaceServer.Models.Execution;
using static Pocket.Logger<MLS.Agent.Controllers.DotnetWorkspaceController>;

namespace MLS.Agent.Controllers
{
    public class DotnetWorkspaceController : Controller
    {
        private readonly WorkspaceServerRegistry workspaceServerRegistry;

        public DotnetWorkspaceController(WorkspaceServerRegistry workspaceServerRegistry)
        {
            this.workspaceServerRegistry = workspaceServerRegistry;
        }

        [HttpPost]
        [Route("/workspace/{workspaceId}/compile")]
        public async Task<IActionResult> Run(
            string workspaceId,
            [FromBody] RunRequest request)
        {
            using (var operation = Log.ConfirmOnExit())
            {
                var server = await workspaceServerRegistry.GetWorkspaceServer(workspaceId);

                var result = await server.Run(request);

                operation.Succeed();

                return Ok(result);
            }
        }
    }
}
