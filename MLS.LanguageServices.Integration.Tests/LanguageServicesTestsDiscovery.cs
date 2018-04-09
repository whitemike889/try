using System;
using Peaky.Client;
using Peaky.XUnit;

namespace MLS.LanguageServices.Integration.Tests
{
    public class LanguageServicesTestsDiscovery : PeakyXunitTestBase, IDisposable
    {
        private static readonly Uri TestDiscoveryUri = new Uri("https://mls-monitoring.azurewebsites.net/tests/staging/LanguageServices?deployment=true");

        private readonly PeakyClient _peakyClient = new PeakyClient(TestDiscoveryUri);

        public override PeakyClient PeakyClient => _peakyClient;

        public void Dispose() => _peakyClient.Dispose();
    }
}