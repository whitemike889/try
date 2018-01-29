using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WorkspaceServer.Models.Execution
{
    public class WorkspaceRunRequest
    {
        private const string DefaultWorkspaceType = "script";

        public WorkspaceRunRequest(
        string buffer = null,
        string source = null, // TODO: added for backward comaptibility
        string bufferid = null,
        int position = 0,
        string[] usings = null,
        string[][] files = null,
        string workspaceType = DefaultWorkspaceType)
        {
            WorkspaceType = workspaceType ?? DefaultWorkspaceType;
            Usings = usings ?? Array.Empty<string>();
            Buffer = buffer ?? source ??string.Empty;
            BufferId = bufferid ?? string.Empty;
            Usings = usings ?? Array.Empty<string>();
            Position = position;
            var sourceFiles = files?.Select(entry => SourceFile.Create(entry[0], entry[1])).ToList() ?? new List<SourceFile>();
            if (!string.IsNullOrWhiteSpace(Buffer))
            {
                sourceFiles.Add(SourceFile.Create(Buffer, "Program.cs"));
            }
            SourceFiles = sourceFiles;
        }
        public string BufferId { get; }

        public string Buffer { get; }

        [Required]
        [MinLength(1)]
        public IReadOnlyCollection<SourceFile> SourceFiles { get; }

        public string[] Usings { get; }

        public string WorkspaceType { get; }

        public int Position { get; }
    }
}
