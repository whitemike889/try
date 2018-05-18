using System;

namespace WorkspaceServer
{
    public class WorkspaceInfo
    {
        public string Type { get; set; }
        public DateTimeOffset? BuildTime { get; set; }
        public DateTimeOffset? ConstructionTime { get; set; }
        public DateTimeOffset? PublicationTime { get; set; }
        public DateTimeOffset? CreationTime { get; set; }
    }
}