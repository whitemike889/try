using System;
using Newtonsoft.Json;

namespace Microsoft.DotNet.Try.Client.Configuration
{
    public class ClientConfiguration
    {
        public string VersionId { get; }

        public int DefaultTimeoutMs { get; }

        [JsonProperty("_links")]
        public RequestDescriptors Links { get; }
      
        public string ApplicationInsightsKey { get; }
        public bool EnableBranding { get; }

        public ClientConfiguration(string versionId,
                                   RequestDescriptors links,
                                   int defaultTimeoutMs,
                                   string applicationInsightsKey,
                                   bool enableBranding)
        {
            if (defaultTimeoutMs <= 0)
                throw new ArgumentOutOfRangeException(nameof(defaultTimeoutMs));
            if (string.IsNullOrWhiteSpace(versionId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(versionId));
            VersionId = versionId;
            Links = links ?? throw new ArgumentNullException(nameof(links));
            DefaultTimeoutMs = defaultTimeoutMs;
            ApplicationInsightsKey = applicationInsightsKey ?? throw new ArgumentNullException(nameof(applicationInsightsKey));
            EnableBranding = enableBranding;
        }
    }
}