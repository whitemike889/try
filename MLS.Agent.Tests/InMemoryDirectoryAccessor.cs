using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            _rootDirToAddFiles = rootDirectoryToAddFiles?? TestAssets.SampleConsole;
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
            return _files.ContainsKey(ResolveFilePath(filePath));
        }

        public string ReadAllText(string filePath)
        {
            _files.TryGetValue(ResolveFilePath(filePath), out var value);
            return value;
        }

        private string ResolveFilePath(string fileName)
        {
            var path = Path.IsPathRooted(fileName) ?
                fileName :
                Path.Combine(_workingDirectory.FullName, fileName);
            return path.NormalizePath();
        }

        public IEnumerator GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}