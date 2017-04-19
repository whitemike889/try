using System;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace MLS.Agent.Tests
{
    public class ApiViaHttpTests
    {
        private readonly ITestOutputHelper output;

        public ApiViaHttpTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        private IWebHostBuilder CreateWebHostBuilder()
        {
            var config = new ConfigurationBuilder().Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<Startup>();

            return host;
        }

        [Fact]
        public async Task hello_world()
        {
            using (var server = CreateTestServer())
            using (var client = server.CreateClient())
            {
                var request = new HttpRequestMessage(new HttpMethod("GET"), "/api/hello");

                var response = await client.SendAsync(request);

                response.ShouldIndicateSuccess();

                var content = await response.Content.ReadAsStringAsync();

                content.Should().Be("Hello!");
            }
        }

        private TestServer CreateTestServer()
        {
            return new TestServer(CreateWebHostBuilder());
        }

        [Fact]
        public async Task Code_sample_can_be_retrieved_using_a_gist_url()
        {
            var gistUri =
                @"https://gist.githubusercontent.com/jonsequitur/b36ec7591de8f2fa58b99278953cd557/raw/836753209d481333d9cf3d01d9897b1e1cad8eb6/deferred%2520execution%2520of%2520Select";

            var uri = $"/api/code?from={UrlEncoder.Default.Encode(gistUri)}";

            output.WriteLine(uri);

            using (var server = CreateTestServer())
            using (var client = server.CreateClient())
            {
                var request = new HttpRequestMessage(new HttpMethod("GET"), uri);

                var response = await client.SendAsync(request);

                response.ShouldIndicateSuccess();

                var content = await response.Content.ReadAsStringAsync();

                content.Should().Contain("public class DeferredExecutionExample");
            }
        }
    }
}
