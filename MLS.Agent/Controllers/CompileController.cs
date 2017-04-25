using System.Net.Http;
using System.Threading.Tasks;
using LanguageServer;
using Microsoft.AspNetCore.Mvc;

namespace MLS.Agent.Controllers
{
    [Route("api/[controller]")]
    public class CompileController : Controller
    {
        public async Task<ProcessResult> Post([FromBody] CompileAndExecuteRequest request)
        {
            var languageServer = new DotDotnetLanguageServer();

            return await languageServer.CompileAndExecute(request);
        }
    }

    [Route("api/[controller]")]
    public class CodeController : Controller
    {
        public async Task<string> Get(string from)
        {
            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    from);

                var response = await httpClient.SendAsync(request);

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
