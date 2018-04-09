using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Peaky.Client;
using Peaky.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace MLS.Agent.Integration.Tests
{
    public class AgentTestsDiscovery : PeakyXunitTestBase, IDisposable
    {
        private static readonly Uri TestDiscoveryUri = new Uri("https://mls-monitoring.azurewebsites.net/tests/staging/agent?deployment=true");

        private readonly PeakyClient _peakyClient = new PeakyClient(TestDiscoveryUri);

        public override PeakyClient PeakyClient => _peakyClient;

        public void Dispose() => _peakyClient.Dispose();
    }

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
            var result = await _peakyClient.GetResultFor(url);

            _output.WriteLine(result.Content);

            result.Passed.Should().BeTrue();
        }

        public void Dispose() => _peakyClient.Dispose();
    }
}
