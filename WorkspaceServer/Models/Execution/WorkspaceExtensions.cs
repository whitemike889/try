using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using MLS.Agent.Tools;
using MLS.Protocol.Execution;

namespace WorkspaceServer.Models.Execution
{
    public static class WorkspaceExtensions
    {
        public static IReadOnlyCollection<SourceFile> GetSourceFiles(this Workspace workspace)
        {
            return workspace.Files?.Select(f => SourceFile.Create(f.Text, f.Name)).ToArray() ?? Array.Empty<SourceFile>();
        }

        public static Workspace.File GetFileFromBufferId(this Workspace workspace, BufferId bufferId)
        {
            if (bufferId == null)
            {
                throw new ArgumentNullException(nameof(bufferId));
            }

            return workspace.Files.FirstOrDefault(f => f.Name == bufferId.FileName);
        }

        public static int GetAbsolutePositionForGetBufferWithSpecifiedIdOrSingleBufferIfThereIsOnlyOne(
            this Workspace workspace,
            BufferId bufferId = null)
        {
            // TODO: (GetAbsolutePositionForGetBufferWithSpecifiedIdOrSingleBufferIfThereIsOnlyOne) this concept should go away

            var buffer = workspace.GetBufferWithSpecifiedIdOrSingleBufferIfThereIsOnlyOne(bufferId);

            return buffer.AbsolutePosition;
        }

        

        internal static (int line, int column, int absolutePosition) GetTextLocation(
            this Workspace workspace,
            BufferId bufferId)
        {
            var file = workspace.GetFileFromBufferId(bufferId);
            var absolutePosition = GetAbsolutePositionForGetBufferWithSpecifiedIdOrSingleBufferIfThereIsOnlyOne(workspace, bufferId);

            var src = SourceText.From(file.Text);
            var line = src.Lines.GetLineFromPosition(absolutePosition);

            return (line: line.LineNumber, column: absolutePosition - line.Start, absolutePosition);
        }

        public static Workspace AddBuffer(
            this Workspace workspace,
            string id,
            string text) =>
            new Workspace(
                workspace.Usings,
                workspace.Files,
                workspace.Buffers.Concat(new[] { new Workspace.Buffer(BufferId.Parse(id), text) }).ToArray(),
                workspace.WorkspaceType,
                workspace.IncludeInstrumentation);

        public static Workspace RemoveBuffer(
            this Workspace workspace,
            string id) =>
            new Workspace(
                workspace.Usings,
                workspace.Files,
                workspace.Buffers.Where(b => b.Id.ToString() != id).ToArray(),
                workspace.WorkspaceType,
                workspace.IncludeInstrumentation);

        public static Workspace ReplaceBuffer(
            this Workspace workspace,
            string id,
            string text) =>
            workspace.RemoveBuffer(id).AddBuffer(id, text);

        public static Workspace ReplaceFile(
            this Workspace workspace,
            string name,
            string text) =>
            new Workspace(
                workspace.Usings,
                workspace.Files
                         .Where(f => f.Name != name)
                         .Concat(new[] { new Workspace.File(name, text) })
                         .ToArray(),
                workspace.Buffers,
                workspace.WorkspaceType,
                workspace.IncludeInstrumentation);


        public static Workspace FromDirectory(DirectoryInfo directory, string workspaceType)
        {
            var filesOnDisk = directory.GetFiles("*.cs", SearchOption.AllDirectories)
                                       .Where(f => !f.IsBuildOutput())
                                       .ToArray();

            if (!filesOnDisk.Any())
            {
                throw new ArgumentException("Directory does not contain any .cs files.");
            }

            var files = filesOnDisk.Select(file => new MLS.Protocol.Execution.Workspace.File(file.Name, file.Read())).ToList();

            return new Workspace(
                files: files.ToArray(),
                buffers: new[]
                {
                    new MLS.Protocol.Execution.Workspace.Buffer(
                        BufferId.Parse(files.First().Name),
                        filesOnDisk.First().Read(),
                        0)
                },
                workspaceType: workspaceType);
        }
    }
}
