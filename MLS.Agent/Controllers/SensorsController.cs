using System;
using System.IO;
using System.Linq;
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

        //[Route("/sensors/getNugetCache")]
        public IActionResult GetNugetCache()
        {
            var dirs = Directory.EnumerateFileSystemEntries(Paths.NugetCache,"*.dll", SearchOption.AllDirectories ).OrderBy(path => path).ToArray();
            return Ok(dirs);
        }

        //[Route("/sensors/environmentVariables")]
        public IActionResult GetEnvironmentVariables()
        {
            return Ok(Environment.GetEnvironmentVariables());
        }
    }
}
