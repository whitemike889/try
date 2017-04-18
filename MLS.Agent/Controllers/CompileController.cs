using System.Threading.Tasks;
using LanguageServer;
using Microsoft.AspNetCore.Mvc;

namespace MLS.Agent.Controllers
{
    [Route("api/[controller]")]
    public class CompileController : Controller
    {
        public async Task<CompileAndExecuteResult> Post([FromBody] CompileAndExecuteRequest request)
        {
            var languageServer = new DotDotnetLanguageServer();

            return await languageServer.CompileAndExecute(request);
        }
    }
}