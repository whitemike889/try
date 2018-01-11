using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkspaceServer.Models.Execution
{
    public class RunRequest
    {
        public RunRequest(string source, string[] usings = null)
        {
            SourceFiles = string.IsNullOrWhiteSpace(source)
                              ? Array.Empty<SourceFile>()
                              : new[] { SourceFile.Create(source, "Program.cs") };

            Usings = usings ?? Array.Empty<string>();
        }

        [Required]
        [MinLength(1)]
        public IReadOnlyCollection<SourceFile> SourceFiles { get; }

        public string[] Usings { get; }
    }
}
