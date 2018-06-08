using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MLS.Agent.Tools;
using Recipes;
using WorkspaceServer;

namespace MLS.Agent.Controllers
{
    public class SensorsController : Controller
    {
        private readonly DotnetWorkspaceServerRegistry _workspaceServerRegistry;

        public SensorsController(DotnetWorkspaceServerRegistry workspaceServerRegistry)
        {
            _workspaceServerRegistry = workspaceServerRegistry ?? throw new ArgumentNullException(nameof(workspaceServerRegistry));
        }

        [Route("/sensors/version")]
        public IActionResult GetVersion() => Ok(AssemblyVersionSensor.Version());

        [Route("/sensors/workspaceInfo")]
        public IActionResult GetWorkspaceInfo()
        {
            var info = _workspaceServerRegistry.GetRegisterWorkspaceInfos();
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
