using System;
using Microsoft.AspNetCore.Mvc;
using Recipes;

namespace MLS.Agent.Controllers
{
    public class SensorsController : Controller
    {
        [Route("/sensors/version")]
        public IActionResult GetVersion() => Ok(VersionSensor.Version());
    }
}