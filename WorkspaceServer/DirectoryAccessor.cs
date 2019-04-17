using System.IO;
using Microsoft.DotNet.Try.Markdown;

namespace WorkspaceServer
{
    public static class DirectoryAccessor
    {
        public static bool FileExists(
            this IDirectoryAccessor directoryAccessor,
            string relativePath) =>
            directoryAccessor.FileExists(new RelativeFilePath(relativePath));

        public static string ReadAllText(
            this IDirectoryAccessor directoryAccessor,
            string relativePath) =>
            directoryAccessor.ReadAllText(new RelativeFilePath(relativePath));

        public static IDirectoryAccessor GetDirectoryAccessorForRelativePath(
            this IDirectoryAccessor directoryAccessor,
            string relativePath) =>
            directoryAccessor.GetDirectoryAccessorForRelativePath(new RelativeDirectoryPath(relativePath));

        public static DirectoryInfo GetFullyQualifiedRoot(this IDirectoryAccessor directoryAccessor) =>
            (DirectoryInfo) directoryAccessor.GetFullyQualifiedPath(new RelativeDirectoryPath("."));
    }
}