using System.IO;
using MLS.Agent.Markdown;
using MLS.Agent.Tests.Markdown;
using WorkspaceServer;
using WorkspaceServer.Tests;

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
                if (absolutePath is FileInfo file && !file.Directory.Exists)
                {
                    file.Directory.Create();
                }
                File.WriteAllText(absolutePath.FullName, text);
            }

            return new FileSystemDirectoryAccessor(inMemoryDirectoryAccessor.WorkingDirectory);
        }
    }
}