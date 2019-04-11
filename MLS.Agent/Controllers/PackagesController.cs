using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WorkspaceServer;
using WorkspaceServer.Packaging;

namespace MLS.Agent.Controllers
{
    public class PackagesController : Controller
    {
        private readonly PackageRegistry _registry;

        public PackagesController(PackageRegistry registry)
        {
            _registry = registry;
        }

        [HttpGet]
        [Route("/packages/{name}/{version}")]
        public async Task<IActionResult> GetPackage(string name, string version)
        {
            try
            {
                var package = (Package) await _registry.Get(name);
                var isBlazorSupported = package.CanSupportBlazor();
                return Ok(value: new Microsoft.DotNet.Try.Protocol.Packaging.Package(isBlazorSupported));
            }
            catch (PackageNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}