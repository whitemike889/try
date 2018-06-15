using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Clockwise;
using Pocket;
using Recipes;
using WorkspaceServer.Models;
using Xunit.Abstractions;

namespace MLS.Agent.Tests
{
    public abstract class ApiViaHttpTestsBase : IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        protected ApiViaHttpTestsBase(ITestOutputHelper output)
        {
            _disposables.Add(output.SubscribeToPocketLogger());
            _disposables.Add(VirtualClock.Start());
        }

        public void Dispose() => _disposables.Dispose();

        protected static async Task<HttpResponseMessage> CallRun(
            string content,
            int? runTimeoutMs = null, 
            CommandLineOptions options = null)
        {
            HttpResponseMessage response;
            using (var agent = new AgentService(options))
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/run")
                {
                    Content = new StringContent(
                        content,
                        Encoding.UTF8,
                        "application/json")
                };

                if (runTimeoutMs != null)
                {
                    request.Headers.Add("Timeout", runTimeoutMs.Value.ToString("F0"));
                }

                response = await agent.SendAsync(request);
            }

            return response;
        }

        protected static Task<HttpResponseMessage> CallRun(
            WorkspaceRequest request,
            int? runTimeoutMs = null)
        {
            return CallRun(request.ToJson(), runTimeoutMs);
        }

        protected static async Task<HttpResponseMessage> CallSignatureHelp(
            string request,
            int? runTimeoutMs = null)
        {
            HttpResponseMessage response;
            using (var agent = new AgentService(null))
            {
                var request1 = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/signaturehelp")
                {
                    Content = new StringContent(
                        request,
                        Encoding.UTF8,
                        "application/json")
                };

                if (runTimeoutMs != null)
                {
                    request1.Headers.Add("Timeout", runTimeoutMs.Value.ToString("F0"));
                }

                response = await agent.SendAsync(request1);
            }

            return response;
        }
    }
}