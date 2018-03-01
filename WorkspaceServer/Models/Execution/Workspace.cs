using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using MLS.Agent.Tools;
using WorkspaceServer.Servers.Dotnet;

namespace WorkspaceServer.Models.Execution
{

    public static class WorkspaceExtensions
    {
        public static IReadOnlyCollection<SourceFile> GetSourceFiles(this Workspace workspace)
        {
            return workspace.Files?.Select(f => SourceFile.Create(f.Text, f.Name)).ToArray() ?? Array.Empty<SourceFile>();
        }
    }
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
            var code = buffer ?? source ?? string.Empty;
            var id = bufferid ?? string.Empty;

            Usings = usings ?? Array.Empty<string>();

            Files = files?? Array.Empty<File>();
            
            var bufferList = buffers?.ToList() ?? new List<Buffer>();

            if (!string.IsNullOrWhiteSpace(code))
            {
                bufferList.Add(new Buffer(id,code,position));
            }
            Buffers = bufferList;
        }
        
        public IReadOnlyCollection<File> Files { get; }

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

        public static Workspace FromDirectory(DirectoryInfo directory, string workspaceType)
        {
            var filesOnDisk = directory.GetFiles("*.cs");

            if (!filesOnDisk.Any())
            {
                throw new ArgumentException("Directory does not contain any .cs files.");
            }

            var files = filesOnDisk.Select(file => new File(file.Name, file.Read())).ToList();

            return new Workspace(
                files: files.ToArray(),
                buffers: new[]
                {
                    new Buffer(
                        files.First().Name,
                        filesOnDisk.First().Read(),
                        0)
                },
                workspaceType: workspaceType);
        }
    }
}
