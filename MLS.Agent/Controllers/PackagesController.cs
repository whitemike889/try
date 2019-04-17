using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.DotNet.Try.Client.Configuration;
using WorkspaceServer;
using WorkspaceServer.Packaging;

namespace MLS.Agent.Controllers
{
    public class PackagesController : Controller
    {
        private const string GetPackageRoute = "/packages/{name}/{version}";
        public static RequestDescriptor GetPackageApi => new RequestDescriptor(
            GetPackageRoute,
            timeoutMs: 60000,
            method: "GET",
            templated: true);

        private readonly PackageRegistry _registry;

        public PackagesController(PackageRegistry registry)
        {
            _registry = registry;
        }

        [HttpGet]
        [Route(GetPackageRoute)]
        public async Task<IActionResult> GetPackage(string name, string version)
        {
            try
            {
                var package = await _registry.Get<IMayOrMayNotSupportBlazor>(name);
                var isBlazorSupported = package.CanSupportBlazor;
                return Ok(value: new Microsoft.DotNet.Try.Protocol.Package(isBlazorSupported));
            }
            catch (PackageNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}