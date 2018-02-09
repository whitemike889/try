using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WorkspaceServer.Models.Execution
{
    public class Workspace
    {
        private const string DefaultWorkspaceType = "script";

        public Workspace(
        string buffer = null, // TODO: added for backward comaptibility
        string source = null, // TODO: added for backward comaptibility
        string bufferid = null, // TODO: added for backward comaptibility
        int position = 0,
        string[] usings = null,
        File[] files = null,
        Buffer[] buffers = null,
        string workspaceType = DefaultWorkspaceType)
        {
            WorkspaceType = workspaceType ?? DefaultWorkspaceType;
            Usings = usings ?? Array.Empty<string>();
            var code  = buffer ?? source ??string.Empty;
            var id = bufferid ?? string.Empty;

            Usings = usings ?? Array.Empty<string>();
          
            var sourceFiles = files?.Select(entry => SourceFile.Create(entry.Text, entry.Name)).ToList() ?? new List<SourceFile>();
            
            var bufferList = buffers?.ToList() ?? new List<Buffer>();

            if (!string.IsNullOrWhiteSpace(code))
            {
                bufferList.Add(new Buffer(id,code,position));
                sourceFiles.Add(SourceFile.Create(code, "Program.cs"));
            }

            Buffers = bufferList;
            if (sourceFiles.Count == 0)
            {
                sourceFiles.Add(SourceFile.Create(bufferList[0].Content, "Program.cs"));
            }
            SourceFiles = sourceFiles;
        }

        [Required]
        public IReadOnlyCollection<SourceFile> SourceFiles { get; }

        public string[] Usings { get; }

        public string WorkspaceType { get; }

        [Required]
        [MinLength(1)]
        public IReadOnlyCollection<Buffer> Buffers { get; }

        public class File
        {
            public File(string name, string text)
            {
                Name = name;
                Text = text;
            }

            public string Name { get; }
            public string Text { get; }
        }

        public class Buffer
        {
            public Buffer(string id, string content, int position)
            {
                Id = id;
                Content = content;
                Position = position;
            }

            public string Id { get; }
            public string Content { get; }

            public int Position { get; }
        }
    }
}
