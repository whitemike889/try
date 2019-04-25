using System.Net.Http;
using System.Threading.Tasks;

namespace MLS.Agent.Tests
{
    public static class AgentServiceExtensions
    {
        public static Task<HttpResponseMessage> GetAsync(
            this AgentService service,
            string uri, 
            string referer = null)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, uri);
            if (referer != null)
            {
                message.Headers.Add("referer", referer);
            }

            return service.SendAsync(message);
        }
    }
}