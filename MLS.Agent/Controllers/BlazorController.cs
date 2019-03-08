using System;
using Microsoft.AspNetCore.Mvc;
using Recipes;
using WorkspaceServer;

namespace MLS.Agent.Controllers
{
    public class BlazorController : Controller
    {
        private readonly PackageRegistry packageRegistry;

        public BlazorController(PackageRegistry packageRegistry)
        {
            this.packageRegistry = packageRegistry ?? throw new ArgumentNullException(nameof(packageRegistry));
        }

        //[Route("/LocalCodeRunner/")]
        public IActionResult GetWorkspaceInfo()
        {
            var info = packageRegistry.GetRegisteredPackageInfos();
            return Ok(info);
        }
    }
}
