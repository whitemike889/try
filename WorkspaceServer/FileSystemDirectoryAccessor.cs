using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Try.Markdown;
using WorkspaceServer.Servers.Roslyn;

namespace WorkspaceServer
{
    public class FileSystemDirectoryAccessor : IDirectoryAccessor
    {
        private readonly DirectoryInfo _rootDirectory;

        public FileSystemDirectoryAccessor(DirectoryInfo rootDir)
        {
            _rootDirectory = rootDir ?? throw new ArgumentNullException(nameof(rootDir));
        }

        public bool FileExists(RelativeFilePath filePath)
        {
            return GetFullyQualifiedPath(filePath).Exists;
        }  

        public string ReadAllText(RelativeFilePath filePath)
        {
            return File.ReadAllText(GetFullyQualifiedPath(filePath).FullName);
        }

        public FileSystemInfo GetFullyQualifiedPath(RelativePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            switch (path)
            {
                case RelativeFilePath file:
                    return new FileInfo(
                        _rootDirectory.Combine(file).FullName);
                case RelativeDirectoryPath dir:
                    return new DirectoryInfo(
                        _rootDirectory.Combine(dir).FullName);
                default:
                    throw new NotSupportedException($"{path.GetType()} is not supported.");
            }
        }

        public IDirectoryAccessor GetDirectoryAccessorForRelativePath(RelativeDirectoryPath relativePath)
        {
            var absolutePath = _rootDirectory.Combine(relativePath).FullName;
            return new FileSystemDirectoryAccessor(new DirectoryInfo(absolutePath));
        }

        public IEnumerable<RelativeFilePath> GetAllFilesRecursively()
        {
            var files = _rootDirectory.GetFiles("*", SearchOption.AllDirectories);
            return files.Select(f =>
             new RelativeFilePath(PathUtilities.GetRelativePath(_rootDirectory.FullName, f.FullName)));
        }
    }
}
