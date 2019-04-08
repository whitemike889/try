using System;
using Newtonsoft.Json;

namespace Microsoft.DotNetTry.Protocol.ClientApi
{
    public class SourceFile
    {
        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("content")]
        public string Content { get; }

        public SourceFile(string name, string content)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            Name = name;
            Content = content ?? string.Empty;
        }
    }
}