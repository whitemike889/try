using System.Collections.Generic;
using System.IO;
using Microsoft.DotNet.Try.Markdown;

namespace WorkspaceServer
{
    public interface IDirectoryAccessor
    {
        bool FileExists(RelativeFilePath filePath);
        string ReadAllText(RelativeFilePath filePath);
        IEnumerable<RelativeFilePath> GetAllFilesRecursively();
        FileSystemInfo GetFullyQualifiedPath(RelativePath path);
        IDirectoryAccessor GetDirectoryAccessorForRelativePath(RelativeDirectoryPath relativePath);
    }
}