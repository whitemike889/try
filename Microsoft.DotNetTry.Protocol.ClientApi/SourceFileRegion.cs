using System;
using Newtonsoft.Json;

namespace Microsoft.DotNetTry.Protocol.ClientApi
{
    public class SourceFileRegion
    {
        [JsonProperty("id")]
        public string Id { get; }
        [JsonProperty("content")]
        public string Content { get; }

        public SourceFileRegion(string id, string content)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
            }

            Id = id;
            Content = content ?? string.Empty;
        }
    }
}