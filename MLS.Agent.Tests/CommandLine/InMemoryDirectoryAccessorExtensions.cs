using System.IO;
using MLS.Agent.Markdown;
using MLS.Agent.Tests.Markdown;

namespace MLS.Agent.Tests.CommandLine
{
    internal static class InMemoryDirectoryAccessorExtensions
    {
        public static FileSystemDirectoryAccessor CreateFiles(this InMemoryDirectoryAccessor inMemoryDirectoryAccessor)
        {
            foreach (var filePath in inMemoryDirectoryAccessor.GetAllFilesRecursively())
            {
                var absolutePath = inMemoryDirectoryAccessor.GetFullyQualifiedPath(filePath);
                var text = inMemoryDirectoryAccessor.ReadAllText(filePath);
                File.WriteAllText(absolutePath.FullName, text);
            }

            return new FileSystemDirectoryAccessor(inMemoryDirectoryAccessor.WorkingDirectory);
        }
    }
}