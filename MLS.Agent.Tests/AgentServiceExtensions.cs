using System.Net.Http;
using System.Threading.Tasks;

namespace MLS.Agent.Tests
{
    public static class AgentServiceExtensions
    {
        public static Task<HttpResponseMessage> GetAsync(
            this AgentService service,
            string uri) =>
            service.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri));
    }
}