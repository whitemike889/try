using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkspaceServer.Models.Execution
{
    public class RunRequest
    {
        private const string DefaultWorkspaceType = "script";

        public RunRequest(
            string source,
            string[] usings = null,
            string workspaceType = DefaultWorkspaceType)
        {
            SourceFiles = string.IsNullOrWhiteSpace(source)
                              ? Array.Empty<SourceFile>()
                              : new[] { SourceFile.Create(source, "Program.cs") };

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
