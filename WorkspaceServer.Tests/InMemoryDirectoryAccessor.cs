using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Try.Markdown;

namespace WorkspaceServer.Tests
{
    public class InMemoryDirectoryAccessor : IDirectoryAccessor, IEnumerable
    {
        private readonly DirectoryInfo _rootDirToAddFiles;

        private Dictionary<FileSystemInfo, string> _files = new Dictionary<FileSystemInfo, string>(
            new Dictionary<FileSystemInfo, string>(),
            new FileSystemInfoComparer());

        public InMemoryDirectoryAccessor(
            DirectoryInfo workingDirectory = null,
            DirectoryInfo rootDirectoryToAddFiles = null)
        {
            WorkingDirectory = workingDirectory ??
                               new DirectoryInfo(Path.Combine("some", "fake", "path"));

            _rootDirToAddFiles = rootDirectoryToAddFiles ??
                                 WorkingDirectory;
        }

        public DirectoryInfo WorkingDirectory { get; }

        public void Add((string path, string content) file)
        {
            var fileInfo = new FileInfo(Path.Combine(_rootDirToAddFiles.FullName, file.path));

            _files.Add(fileInfo, file.content);
        }

        public bool DirectoryExists(RelativeDirectoryPath path)
        {
            var fullyQualifiedDirPath = GetFullyQualifiedPath(path);

            return _files
                   .Keys
                   .Any(f =>
                   {
                       switch (f)
                       {
                           case FileInfo file:
                               return FileSystemInfoComparer.Instance.Equals(
                                   file.Directory,
                                   fullyQualifiedDirPath);

                           case DirectoryInfo dir:
                               return FileSystemInfoComparer.Instance.Equals(
                                   dir,
                                   fullyQualifiedDirPath);

                           default:
                               throw new NotSupportedException();
                       }
                   });
        }

        public void EnsureDirectoryExists(RelativeDirectoryPath path)
        {
            _files[GetFullyQualifiedPath(path)] = null;
        }

        public bool FileExists(RelativeFilePath path)
        {
            return _files.ContainsKey(GetFullyQualifiedPath(path));
        }

        public string ReadAllText(RelativeFilePath path)
        {
            _files.TryGetValue(GetFullyQualifiedPath(path), out var value);
            return value;
        }

        public void WriteAllText(RelativeFilePath path, string text)
        {
            _files[GetFullyQualifiedPath(path)] = text;
        }

        public FileSystemInfo GetFullyQualifiedPath(RelativePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException();
            }

            switch (path)
            {
                case RelativeFilePath rfp:
                    return WorkingDirectory.Combine(rfp);
                case RelativeDirectoryPath rdp:
                    return WorkingDirectory.Combine(rdp);
                default:
                    throw new NotSupportedException();
            }
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public IDirectoryAccessor GetDirectoryAccessorForRelativePath(RelativeDirectoryPath relativePath)
        {
            var newPath = WorkingDirectory.Combine(relativePath);
            return new InMemoryDirectoryAccessor(newPath)
                   {
                       _files = _files
                   };
        }

        public IEnumerable<RelativeFilePath> GetAllFilesRecursively()
        {
            return _files.Keys.Select(key => new RelativeFilePath(
                                          Path.GetRelativePath(WorkingDirectory.FullName, key.FullName)));
        }

        internal class FileSystemInfoComparer : IEqualityComparer<FileSystemInfo>
        {
            public static FileSystemInfoComparer Instance { get; } = new FileSystemInfoComparer();

            public bool Equals(FileSystemInfo x, FileSystemInfo y)
            {
                if (x.GetType() == y.GetType())
                {
                    return x is DirectoryInfo
                               ? RelativePath.NormalizeDirectory(x.FullName) == RelativePath.NormalizeDirectory(y.FullName)
                               : x.FullName == y.FullName;
                }

                return false;
            }

            public int GetHashCode(FileSystemInfo obj)
            {
                var fullName = obj.FullName;

                if (obj is DirectoryInfo)
                {
                    fullName = RelativePath.NormalizeDirectory(fullName);
                }

                var hashCode = $"{obj.GetType().GetHashCode()}:{fullName}".GetHashCode();

                return hashCode;
            }
        }
    }
}