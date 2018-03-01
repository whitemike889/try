using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WorkspaceServer.WorkspaceFeatures
{
    public class WebServer : IDisposable
    {
        private readonly HttpClient _httpClient;

        public WebServer(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return await _httpClient.SendAsync(request);
        }

        public void Dispose()
        {
        }
    }
}
