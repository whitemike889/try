using System;
using Newtonsoft.Json;

namespace Microsoft.DotNetTry.Protocol.ClientApi
{
    public class CreateRegionsFromFilesResponse : MessageBase
    {
        [JsonProperty("regions")]
        public SourceFileRegion[] Regions { get; }

        public CreateRegionsFromFilesResponse(string requestId, SourceFileRegion[] regions) : base(requestId)
        {
            Regions = regions ?? Array.Empty<SourceFileRegion>();
        }
    }
}