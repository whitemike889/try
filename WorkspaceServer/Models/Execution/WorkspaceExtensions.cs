using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace WorkspaceServer.Models.Execution
{
    public static class WorkspaceExtensions
    {
        public static IReadOnlyCollection<SourceFile> GetSourceFiles(this Workspace workspace)
        {
            return workspace.Files?.Select(f => SourceFile.Create(f.Text, f.Name)).ToArray() ?? Array.Empty<SourceFile>();
        }

        public static Workspace.File GetFileFromBufferId(this Workspace workspace, string bufferId)
        {
            var parsed = bufferId?.Split('@')[0].Trim();
            return workspace.Files.FirstOrDefault(f => f.Name == parsed);
        }

        public static int GetAbsolutePositionForGetBufferWithSpecifiedIdOrSingleBufferIfThereIsOnlyOne(
            this Workspace workspace,
            string bufferId = null)
        {
            // TODO: (GetAbsolutePositionForGetBufferWithSpecifiedIdOrSingleBufferIfThereIsOnlyOne) this concept should go away

            var buffer = GetBufferWithSpecifiedIdOrSingleBufferIfThereIsOnlyOne(workspace, bufferId);

            return buffer.AbsolutePosition;
        }

        public static Workspace.Buffer GetBufferWithSpecifiedIdOrSingleBufferIfThereIsOnlyOne(
            this Workspace workspace,
            string bufferId = null)
        {
            // TODO: (GetBufferWithSpecifiedIdOrSingleBufferIfThereIsOnlyOne) this concept should go away

            var buffer = workspace.Buffers.SingleOrDefault(b => b.Id == bufferId);

            if (buffer == null)
            {
                if (workspace.Buffers.Length == 1)
                {
                    buffer = workspace.Buffers.Single();
                }
                else
                {
                    throw new ArgumentException("Ambiguous buffer");
                }
            }

            return buffer;
        }

        internal static (int line, int column, int absolutePosition) GetTextLocation(
            this Workspace workspace,
            string bufferId)
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
                workspace.Buffers.Concat(new[] { new Workspace.Buffer(id, text) }).ToArray(),
                workspace.WorkspaceType,
                workspace.IncludeInstrumentation);

        public static Workspace RemoveBuffer(
            this Workspace workspace,
            string id) =>
            new Workspace(
                workspace.Usings,
                workspace.Files,
                workspace.Buffers.Where(b => b.Id != id).ToArray(),
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
    }
}
