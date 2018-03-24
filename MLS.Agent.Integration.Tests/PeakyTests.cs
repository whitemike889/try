using System;
using Xunit;
using Peaky.XUnit;
using FluentAssertions;
using Peaky.Client;

namespace MLS.Orchestrator.Integration.Tests
{
    public class PeakyTests : PeakyXunitTestBase, IDisposable
    {
        private readonly PeakyClient _peakyClient = new PeakyClient(new Uri("https://mls-monitoring.azurewebsites.net/tests/staging/agent"));

        public override PeakyClient PeakyClient => _peakyClient;
        

        [Theory]
        [ClassData(typeof(PeakyTests))]
        public async void The_peaky_test_passes(Uri url)
        {
            var result = await PeakyClient.GetResultFor(url);
            
            result.Passed.Should().BeTrue();
        }
        
        public void Dispose()
        {
            _peakyClient.Dispose();
        }
    }
}
