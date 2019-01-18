using System;
using Microsoft.AspNetCore.Mvc;
using Recipes;
using WorkspaceServer;

namespace MLS.Agent.Controllers
{
    public class SensorsController : Controller
    {
        private readonly PackageRegistry packageRegistry;

        public SensorsController(PackageRegistry packageRegistry)
        {
            this.packageRegistry = packageRegistry ?? throw new ArgumentNullException(nameof(packageRegistry));
        }

        [Route("/sensors/version")]
        public IActionResult GetVersion() => Ok(VersionSensor.Version());

        [Route("/sensors/workspaceInfo")]
        public IActionResult GetWorkspaceInfo()
        {
            var info = packageRegistry.GetRegisteredPackageInfos();
            return Ok(info);
        }
    }
}
