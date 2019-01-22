﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WorkspaceServer.Servers.Roslyn;

namespace MLS.Agent.Markdown
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
                throw new ArgumentNullException();
            }

            var absolutePath =  Path.Combine(_rootDirectory.FullName, path.Value);

            if (path is RelativeFilePath)
            {
                return new FileInfo(absolutePath);
            }
            else
            {
                return new DirectoryInfo(absolutePath);
            }
        }

        public IDirectoryAccessor GetDirectoryAccessorForRelativePath(RelativeDirectoryPath relativePath)
        {   
            return new FileSystemDirectoryAccessor(new DirectoryInfo(Path.Combine(_rootDirectory.FullName, relativePath.Value)));
        }

        public IEnumerable<RelativeFilePath> GetAllFilesRecursively()
        {
            var files = _rootDirectory.GetFiles("*", SearchOption.AllDirectories);
            return files.Select(f =>
             new RelativeFilePath(PathUtilities.GetRelativePath(_rootDirectory.FullName, f.FullName)));
        }
    }
}
