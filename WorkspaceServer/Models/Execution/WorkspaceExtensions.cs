using System;
using System.Collections.Generic;
using System.IO;
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
            var parsed = bufferId?.Split("@")[0].Trim();
            var fileName = string.IsNullOrWhiteSpace(parsed) ? "Program.cs" : parsed;
            return workspace.Files.FirstOrDefault(f => f.Name == fileName);
        }

        public static FileInfo GetFileInfoFromBufferId(this Workspace workspace, string bufferId, string root = null)
        {
            var file = workspace.GetFileFromBufferId(bufferId);
            var fileFullPath = string.IsNullOrWhiteSpace(root) ? file.Name : Path.Combine(root, file.Name);
            return  new FileInfo(fileFullPath);
        }

        public static int GetAbsolutePosition(this Workspace workspace, string bufferId, int bufferPosition)
        {
            return (workspace.Buffers.FirstOrDefault(b => b.Id == bufferId)?.Position ?? 0) + bufferPosition;
        }
        
        public static (int line, int column, int absolutePosition) GetTextLocation(this Workspace workspace, string bufferId, int bufferPosition)
        {
            var file = workspace.GetFileFromBufferId(bufferId);
            var absolutePosition = GetAbsolutePosition(workspace, bufferId, bufferPosition);

            var src = SourceText.From(file.Text);
            var line = src.Lines.GetLineFromPosition(absolutePosition);

            return (line: line.LineNumber, column: absolutePosition - line.Start, absolutePosition);
        }
    }
}