using System;
using Microsoft.AspNetCore.Mvc;
using Recipes;
using WorkspaceServer;

namespace MLS.Agent.Controllers
{
    public class SensorsController : Controller
    {
        private readonly WorkspaceRegistry workspaceRegistry;

        public SensorsController(WorkspaceRegistry workspaceRegistry)
        {
            this.workspaceRegistry = workspaceRegistry ?? throw new ArgumentNullException(nameof(workspaceRegistry));
        }

        [Route("/sensors/version")]
        public IActionResult GetVersion() => Ok(VersionSensor.Version());

        [Route("/sensors/workspaceInfo")]
        public IActionResult GetWorkspaceInfo()
        {
            var info = workspaceRegistry.GetRegisteredWorkspaceInfos();
            return Ok(info);
        }
    }
}
