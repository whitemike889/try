using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using static System.Environment;

namespace MLS.Protocol.Execution
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

            if (Files.Distinct().Count() != Files.Length )
            {
                throw new ArgumentException($"Duplicate file names:{NewLine}{string.Join(NewLine, Files.Select(f => f.Name))}");
            }
            
            if (Buffers.Distinct().Count() != Buffers.Length )
            {
                throw new ArgumentException($"Duplicate buffer ids:{NewLine}{string.Join(NewLine, Buffers.Select(b => b.Id))}");
            }
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
            public File(string name, string text, int order = 0)
            {
                Name = name;
                Text = text;
                Order = order;
            }

            public string Name { get; }

            public string Text { get; }
            public int Order { get; }

            public override string ToString() => $"{nameof(File)}: {Name}";
        }

        public class Buffer
        {
            private readonly int offSetFromParentBuffer;

            public Buffer(BufferId id, string content, int position = 0, int offSetFromParentBuffer = 0, int order = 0)
            {
                this.offSetFromParentBuffer = offSetFromParentBuffer;
                Id = id ?? throw new ArgumentNullException(nameof(id));
                Content = content;
                Position = position;
                Order = order;
            }

            public BufferId Id { get; }

            public string Content { get; }

            public int Position { get; internal set; }
            public int Order { get; }

            [JsonIgnore]
            public int AbsolutePosition => Position + offSetFromParentBuffer;

            public override string ToString() => $"{nameof(Buffer)}: {Id}";
        }

        public static Workspace FromSource(
            string source,
            string workspaceType,
            string id = "Program.cs",
            string[] usings = null,
            int position = 0)
        {
            return new Workspace(
                workspaceType: workspaceType,
                buffers: new[]
                {
                    new Buffer(BufferId.Parse(id ?? throw new ArgumentNullException(nameof(id))), source, position)
                },
                usings: usings);
        }

        public static Workspace FromSources(
            string workspaceType = null,
            params (string id, string content, int position)[] sources) =>
            new Workspace(
                workspaceType: workspaceType,
                buffers: sources.Select(s => new Buffer(BufferId.Parse(s.id), s.content, s.position)).ToArray());
    }
}
