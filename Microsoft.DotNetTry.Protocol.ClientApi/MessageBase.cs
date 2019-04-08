using System;
using Newtonsoft.Json;

namespace Microsoft.DotNetTry.Protocol.ClientApi
{
    public abstract class MessageBase
    {
        [JsonProperty("requestId")]
        public string RequestId { get; }

        protected MessageBase(string requestId)
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(requestId));
            }

            RequestId = requestId;
        }
    }
}
