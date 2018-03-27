using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace WorkspaceServer.Models.Execution
{
    public static class WorkspaceExtensions
    {
        public static IReadOnlyCollection<SourceFile> GetSourceFiles(this Workspace workspace)
        {
            return workspace.Files?.Select(f => SourceFile.Create((string) f.Text, (string) f.Name)).ToArray() ?? Array.Empty<SourceFile>();
        }

        public static Workspace.File GetFileFromBufferId(this Workspace workspace, string bufferId)
        {
            var parsed = bufferId?.Split("@")[0].Trim();
            var fileName = string.IsNullOrWhiteSpace(parsed) ? "Program.cs" : parsed;
            var ret = workspace.Files.FirstOrDefault(f => f.Name == fileName);
            return ret;
        }

        public static int GetAbsolutePosition(this Workspace workspace, string bufferId, int bufferPosition)
        {
            var aboslutePosition = workspace.Buffers.FirstOrDefault(b => b.Id == bufferId)?.Position ?? 0 + bufferPosition;
            return aboslutePosition;
        }

        public static (int line, int column) GetLocation(this Workspace workspace, string bufferId, int bufferPosition)
        {
            var file = workspace.GetFileFromBufferId(bufferId);
            var absolutePosition = GetAbsolutePosition(workspace, bufferId, bufferPosition);

            var src = SourceText.From(file.Text);
            var line = src.Lines.GetLineFromPosition(absolutePosition);

            return (line: line.LineNumber, column: absolutePosition - line.Start);
        }
    }
}