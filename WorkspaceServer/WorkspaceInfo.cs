using System;

namespace WorkspaceServer
{
    public class WorkspaceInfo
    {
        public WorkspaceInfo(string type, DateTimeOffset? buildTime, DateTimeOffset? constructionTime, DateTimeOffset? publicationTime, DateTimeOffset? creationTime,
            DateTimeOffset? initialisedTime)
        {
            Type = type;
            BuildTime = buildTime;
            ConstructionTime = constructionTime;
            PublicationTime = publicationTime;
            CreationTime = creationTime;
            InitialisedTime = initialisedTime;
        }

        public string Type { get;  }
        public DateTimeOffset? BuildTime { get;  }
        public DateTimeOffset? ConstructionTime { get;  }
        public DateTimeOffset? PublicationTime { get;  }
        public DateTimeOffset? CreationTime { get;  }
        public DateTimeOffset? InitialisedTime { get; }
    }
}