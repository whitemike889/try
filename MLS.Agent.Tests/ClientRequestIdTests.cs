using System;
using FluentAssertions;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Pocket;
using Xunit;
using Xunit.Abstractions;

namespace MLS.Agent.Tests
{
    public class Given_a_Web_request : IDisposable
    {
        private readonly CompositeDisposable disposables;

        public Given_a_Web_request(ITestOutputHelper output)
        {
            disposables = new CompositeDisposable
            {
                LogEvents.Subscribe(e => output.WriteLine(e.ToLogString()))
            };
        }

        public void Dispose()
        {
            disposables.Dispose();
        }

        [Fact]
        public void When_request_provides_a_ClientRequestId_Then_it_is_echoed_in_the_response()
        {
            var expectedClientRequestId = Guid.NewGuid().ToString();

            using (var agent = new AgentService())
            {
                var response = agent.SendAsync(
                    new HttpRequestMessage(HttpMethod.Get, "/")
                    {
                        Headers =
                        {
                            { "client-request-id", expectedClientRequestId }
                        }
                    });

                response.Result.Headers
                        .Should().ContainSingle(h => h.Key == "client-request-id" && h.Value.Single() == expectedClientRequestId);
            }
        }

        [Fact]
        public void When_request_does_not_provide_a_ClientRequestId_Then_the_service_generates_a_new_value_and_sends_it_in_the_response()
        {
            using (var agent = new AgentService())
            {
                var response = agent.SendAsync(
                    new HttpRequestMessage(HttpMethod.Get, "/"));

                response.Result.Headers
                        .Should().ContainSingle(h => h.Key == "client-request-id" && h.Value.Single() != "");
            }
        }

        [Fact]
        public async void It_should_reflect_the_ClientRequestId_in_logs()
        {
            var log = new LogEntryList();
            string clientRequestId;

            using (var agent = new AgentService())
            using (LogEvents.Subscribe(log.Add))
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    @"/workspace/hello/compile")
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            Source = $@"Console.WriteLine(123);"
                        }),
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await agent.SendAsync(request);

                clientRequestId = response.Headers.Single(h => h.Key == "client-request-id").Value.Single();
            }

            log.Should()
               .Contain(l =>
                            l.Evaluate()
                             .Properties
                             .Any(p => p.Name == "clientRequestId" &&
                                       clientRequestId.Equals(p.Value)));
        }
    }
}
