using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Try.Client.Configuration;
using Microsoft.DotNet.Try.Protocol;
using Recipes;

namespace MLS.Agent.Controllers
{
    public class SensorsController : Controller
    {
        private const string VersionRoute = "/sensors/version";
        public static RequestDescriptor VersionApi => new RequestDescriptor(VersionRoute, method: "GET");

        [Route(VersionRoute)]
        public IActionResult GetVersion() => Ok(VersionSensor.Version());
    }
}