using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LanguageServer
{
    public class DotDotnetLanguageServer
    {
        private readonly HttpClient httpClient;

        public DotDotnetLanguageServer()
        {
            httpClient = new HttpClient();
        }

        public async Task<CompileAndExecuteResult> CompileAndExecute(
            CompileAndExecuteRequest request)
        {
            var x = new HttpRequestMessage(
                HttpMethod.Post,
                "https://www.microsoft.com/net/api/code")
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(request),
                    Encoding.UTF8,
                    "application/json")
            };

            var response = await httpClient.SendAsync(x);

            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine(json);

            return JsonConvert.DeserializeObject<CompileAndExecuteResult>(json);
        }
    }
}
