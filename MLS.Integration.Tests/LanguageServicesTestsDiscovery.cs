// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Peaky.Client;
using Peaky.XUnit;

namespace MLS.Integration.Tests
{
    public class LanguageServicesTestsDiscovery : PeakyXunitTestBase, IDisposable
    {
        private static readonly Uri TestDiscoveryUri = new Uri("https://mls-monitoring.azurewebsites.net/tests/staging/LanguageServices?deployment=true");

        private readonly PeakyClient _peakyClient = new PeakyClient(TestDiscoveryUri);

        public override PeakyClient PeakyClient => _peakyClient;

        public void Dispose() => _peakyClient.Dispose();
    }
}