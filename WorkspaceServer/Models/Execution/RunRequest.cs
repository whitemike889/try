using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkspaceServer.Models.Execution
{
    public class RunRequest
    {
        private const string DefaultWorkspaceType = "script";

        public RunRequest(
            string buffer,
            string[] usings = null,
            string workspaceType = DefaultWorkspaceType)
        {
            SourceFiles = string.IsNullOrWhiteSpace(buffer)
                              ? Array.Empty<SourceFile>()
                              : new[] { SourceFile.Create(buffer, "Program.cs") };

            Usings = usings ?? Array.Empty<string>();

            WorkspaceType = workspaceType ?? DefaultWorkspaceType;
        }

        [Required]
        [MinLength(1)]
        public IReadOnlyCollection<SourceFile> SourceFiles { get; }

        public string[] Usings { get; }

        public string WorkspaceType { get; }
    }
}
