using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LanguageServer
{
    public class DotDotnetLanguageServer : ILanguageServer
    {
        public async Task<ProcessResult> CompileAndExecute(
            CompileAndExecuteRequest request)
        {
            using (var httpClient = new HttpClient())
            {
                var requestMessage = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://www.microsoft.com/net/api/code")
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(request),
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await httpClient.SendAsync(requestMessage);

                var json = await response.Content.ReadAsStringAsync();

                var compileAndExecuteResult = JsonConvert.DeserializeObject<ProcessResult>(json);

                compileAndExecuteResult.Output = compileAndExecuteResult.Output.Select(WebUtility.HtmlDecode).ToArray();

                return compileAndExecuteResult;
            }
        }
    }
}
