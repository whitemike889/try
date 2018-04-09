using System;
using FluentAssertions;
using Peaky.Client;
using Peaky.XUnit;
using Xunit;

namespace MLS.LanguageServices.Integration.Tests
{
    public class LanguageServicesTests : PeakyXunitTestBase, IDisposable
    {
        private readonly PeakyClient _peakyClient = new PeakyClient(new Uri("https://mls-monitoring.azurewebsites.net/tests/staging/LanguageServices?deployment=true"));

        public override PeakyClient PeakyClient => _peakyClient;


        [Theory]
        [ClassData(typeof(LanguageServicesTests))]
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