using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Recipes;

namespace MLS.Agent.Controllers
{
    public class SensorsController : Controller
    {
        [Route("/sensors/version")]
        public async Task<IActionResult> Get() => Ok(AssemblyVersionSensor.Version());
    }
}
