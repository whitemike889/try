using System;
using System.Collections.Generic;

namespace Microsoft.DotNet.Try.Client.Configuration
{
    public class RequestDescriptor
    {
        public int TimeoutMs { get; }
        public string Href { get; }
        public bool Templated { get; }
        public IEnumerable<RequestDescriptorProperty> Properties { get; }
        public string Method { get; }
        public string Body { get; }

        public RequestDescriptor(string href, string method = null, bool templated = false, IReadOnlyCollection<RequestDescriptorProperty> properties = null, string requestBody = null, int timeoutMs = 15000)
        {
            if (string.IsNullOrWhiteSpace(href))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(href));
            }
            TimeoutMs = timeoutMs;
            Href = href;
            Templated = templated;
            Properties = properties ?? Array.Empty<RequestDescriptorProperty>();
            Method = method ?? "POST";
            Body = requestBody;
        }
    }
}