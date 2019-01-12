using System.Collections.Generic;
using System.IO;

namespace MLS.Agent.Markdown
{
    public interface IDirectoryAccessor
    {
        bool FileExists(string filePath);
        string ReadAllText(string filePath);
        IEnumerable<FileInfo> GetAllFilesRecursively();
        string GetFullyQualifiedPath(string path);
        IDirectoryAccessor GetDirectoryAccessorForRelativePath(string relativePath);
    }
}