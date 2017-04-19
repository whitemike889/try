using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace MLS.Agent.Controllers
{
    [Route("api/[controller]")]
    public class HelloController : Controller
    {
        public string Get() => "Hello!";
    }
}
