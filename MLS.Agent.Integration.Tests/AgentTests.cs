using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Peaky.Client;
using Xunit;
using Xunit.Abstractions;

namespace MLS.Agent.Integration.Tests
{
    public class AgentTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        private readonly PeakyClient _peakyClient = new PeakyClient(new HttpClient(){Timeout = TimeSpan.FromMinutes(10)});

        public AgentTests(ITestOutputHelper output)
        {
            _output = output;
        }
        [Theory]
        [ClassData(typeof(AgentTestsDiscovery))]
        public async Task The_peaky_test_passes(Uri url)
        {
             await Task.Delay(10000);
            var result = await _peakyClient.GetResultFor(url);

            if (!result.Passed)
            {
                await Task.Delay(60000);
                result = await _peakyClient.GetResultFor(url);
            }

            _output.WriteLine(result.Content);

            result.Passed.Should().BeTrue();
        }

        public void Dispose() => _peakyClient.Dispose();
    }
}
