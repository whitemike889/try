using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LanguageServer
{
    public class DotDotnetLanguageServer
    {
        public async Task<CompileAndExecuteResult> CompileAndExecute(
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

                return JsonConvert.DeserializeObject<CompileAndExecuteResult>(json);
            }
        }
    }
}
