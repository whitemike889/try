using System;
using Microsoft.AspNetCore.Mvc;
using Recipes;
using WorkspaceServer;

namespace MLS.Agent.Controllers
{
    public class SensorsController : Controller
    {
        private readonly WorkspaceServerRegistry _workspaceServerRegistry;

        public SensorsController(WorkspaceServerRegistry workspaceServerRegistry)
        {
            _workspaceServerRegistry = workspaceServerRegistry;
        }
        [Route("/sensors/version")]
        public IActionResult GetVersion() => Ok(AssemblyVersionSensor.Version());

        [Route("/sensors/workspaceInfo")]
        public IActionResult GetWorkspaceInfo()
        {
            var info = _workspaceServerRegistry.GetRegisterWorkspaceInfos();
            return Ok(info);
        }

        [Route("/sensors/environment")]
        public IActionResult GetEnvirontmentInfo()
        {
            return Ok(Environment.GetEnvironmentVariables());
        }
    }
}
