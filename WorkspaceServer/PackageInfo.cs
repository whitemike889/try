using System;

namespace WorkspaceServer
{
    public class PackageInfo
    {
        public PackageInfo(string type, DateTimeOffset? buildTime, DateTimeOffset? constructionTime, DateTimeOffset? publicationTime, DateTimeOffset? creationTime,
            DateTimeOffset? readyTime, bool blazorSupported = false)
        {
            Type = type;
            BuildTime = buildTime;
            ConstructionTime = constructionTime;
            PublicationTime = publicationTime;
            CreationTime = creationTime;
            ReadyTime = readyTime;
            BlazorSupported = true;
        }

        public string Type { get;  }
        public DateTimeOffset? BuildTime { get;  }
        public DateTimeOffset? ConstructionTime { get;  }
        public DateTimeOffset? PublicationTime { get;  }
        public DateTimeOffset? CreationTime { get;  }
        public DateTimeOffset? ReadyTime { get; }
        public bool BlazorSupported { get; set; }
    }
}