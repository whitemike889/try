using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MLS.Agent.Markdown;

namespace MLS.Agent.Tests
{
    internal class InMemoryDirectoryAccessor : IDirectoryAccessor, IEnumerable
    {
        private readonly DirectoryInfo _rootDirToAddFiles;
        private readonly DirectoryInfo _workingDirectory;
        private Dictionary<string, string> _files;

        public InMemoryDirectoryAccessor(DirectoryInfo workingDirectory, DirectoryInfo rootDirectoryToAddFiles = null)
        {
            _rootDirToAddFiles = rootDirectoryToAddFiles ?? TestAssets.SampleConsole;
            _workingDirectory = workingDirectory;
            _files = new Dictionary<string, string>();
        }

        public void Add((string path, string content) file)
        {
            _files.Add(
                new FileInfo(Path.Combine(_rootDirToAddFiles.FullName, file.path)).FullName, file.content);
        }

        public bool FileExists(string filePath)
        { 
            return _files.ContainsKey(GetFullyQualifiedPath(filePath));
        }

        public string ReadAllText(string filePath)
        {
            _files.TryGetValue(GetFullyQualifiedPath(filePath), out var value);
            return value;
        }

        public string GetFullyQualifiedPath(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException();
            }

            var path = Path.IsPathRooted(filePath) ?
                filePath :
                Path.Combine(_workingDirectory.FullName, filePath);

            var normalizedPath = path.NormalizePath();
            FileSystemDirectoryAccessor.ThrowIfContainsDisallowedCharacters(normalizedPath);
            return normalizedPath;
        }

        public IEnumerator GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public IDirectoryAccessor GetDirectoryAccessorForRelativePath(string relativePath)
        {
            var newPath = Path.Combine(_workingDirectory.FullName, relativePath);
            return new InMemoryDirectoryAccessor(new DirectoryInfo(newPath.NormalizePath()))
            {
                _files = _files
            };
        }

        public IEnumerable<FileInfo> GetAllFilesRecursively()
        {
            return _files.Keys.Select(key => new FileInfo(key));
        }
    }
}