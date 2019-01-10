using System.IO;

namespace MLS.Agent.Markdown
{
    public class FileSystemDirectoryAccessor : IDirectoryAccessor
    {
        private readonly DirectoryInfo _rootDirectory;

        public FileSystemDirectoryAccessor(DirectoryInfo rootDir)
        {
            _rootDirectory = rootDir ?? throw new System.ArgumentNullException(nameof(rootDir));
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(GetFullyQualifiedPath(filePath));
        }

        public string ReadAllText(string filePath)
        {
            return File.ReadAllText(GetFullyQualifiedPath(filePath));
        }

        private string GetFullyQualifiedPath(string filePath)
        {
            var path = Path.IsPathRooted(filePath)
                ? filePath
                : Path.Combine(_rootDirectory.FullName, filePath);

            return path.NormalizePath();

        }
    }
}
