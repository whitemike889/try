using System.Collections.Generic;
using System.IO;
using Microsoft.DotNet.Try.Markdown;

namespace WorkspaceServer
{
    public interface IDirectoryAccessor
    {
        bool FileExists(RelativeFilePath path);

        bool DirectoryExists(RelativeDirectoryPath path);


        void EnsureDirectoryExists(RelativeDirectoryPath path);

        string ReadAllText(RelativeFilePath path);

        void WriteAllText(RelativeFilePath path, string text);

        IEnumerable<RelativeFilePath> GetAllFilesRecursively();

        FileSystemInfo GetFullyQualifiedPath(RelativePath path);

        IDirectoryAccessor GetDirectoryAccessorForRelativePath(RelativeDirectoryPath path);
    }
}