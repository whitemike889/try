using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Servers.Legacy
{
    public class DotDotnetWorkspaceServer : IWorkspaceServer
    {
        public async Task<ProcessResult> CompileAndExecute(RunRequest request)
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

                var result = JsonConvert.DeserializeObject<ProcessResult>(json);

                return new ProcessResult(
                    result.Succeeded,
                    result.Output
                          .Select(WebUtility.HtmlDecode)
                          .ToArray());
            }
        }

        public Task<CompletionResult> GetCompletionList(CompletionRequest request)
        {
            throw new System.NotSupportedException();
        }
    }
}
