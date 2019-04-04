using System.Collections.Generic;
using System.IO;
using Microsoft.DotNet.Try.Markdown;

namespace MLS.Agent.Markdown
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