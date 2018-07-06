using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using MLS.Agent.Tools;
using Newtonsoft.Json;

namespace WorkspaceServer.Models.Execution
{
    public class Workspace
    {
        private const string DefaultWorkspaceType = "script";

        public Workspace(
            string[] usings = null,
            File[] files = null,
            Buffer[] buffers = null,
            string workspaceType = DefaultWorkspaceType,
            bool includeInstrumentation = false)
        {
            WorkspaceType = workspaceType ?? DefaultWorkspaceType;
            Usings = usings ?? Array.Empty<string>();
            Usings = usings ?? Array.Empty<string>();
            Files = files ?? Array.Empty<File>();
            Buffers = buffers ?? Array.Empty<Buffer>();
            IncludeInstrumentation = includeInstrumentation;
        }

        public File[] Files { get; }

        public string[] Usings { get; }

        public string WorkspaceType { get; }

        public bool IncludeInstrumentation { get; }

        [Required]
        [MinLength(1)]
        public Buffer[] Buffers { get; }

        public class File
        {
            public File(string name, string text)
            {
                Name = name;
                Text = text;
            }

            public string Name { get; }

            public string Text { get; }

            public override string ToString() => $"{nameof(File)}: {Name}";
        }

        public class Buffer
        {
            private readonly int offSetFromParentBuffer;

            public Buffer(string id, string content, int position = 0, int offSetFromParentBuffer = 0)
            {
                this.offSetFromParentBuffer = offSetFromParentBuffer;
                Id = id;
                Content = content;
                Position = position;
            }

            public string Id { get; }

            public string Content { get; }

            public int Position { get; }

            [JsonIgnore]
            public int AbsolutePosition => Position + offSetFromParentBuffer;

            public override string ToString() => $"{nameof(Buffer)}: {Id}";
        }

        public static Workspace FromSource(
            string source,
            string workspaceType,
            string[] usings = null,
            string id = null,
            int position = 0) =>
            new Workspace(
                workspaceType: workspaceType,
                buffers: new[]
                {
                    new Buffer(id ?? $"file{source.GetHashCode()}.cs", source, position)
                },
                usings: usings);

        public static Workspace FromSources(
            string workspaceType = null,
            params (string id, string content, int position)[] sources) =>
            new Workspace(
                workspaceType: workspaceType,
                buffers: sources.Select(s => new Buffer(s.id, s.content, s.position)).ToArray());

        public static Workspace FromDirectory(DirectoryInfo directory, string workspaceType)
        {
            var filesOnDisk = directory.GetFiles("*.cs", SearchOption.AllDirectories);

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
