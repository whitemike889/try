using System;
using Peaky.Client;
using Peaky.XUnit;

namespace MLS.Integration.Tests
{
    public class AgentTestsDiscovery : PeakyXunitTestBase, IDisposable
    {
        private static readonly Uri TestDiscoveryUri = new Uri("https://mls-monitoring.azurewebsites.net/tests/staging/agent?deployment=true");

        private readonly PeakyClient _peakyClient = new PeakyClient(TestDiscoveryUri);

        public override PeakyClient PeakyClient => _peakyClient;

        public void Dispose() => _peakyClient.Dispose();
    }
}