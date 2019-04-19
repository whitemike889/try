using System.IO;
using Microsoft.DotNet.Try.Markdown;

namespace WorkspaceServer
{
    public static class DirectoryAccessor
    {
        public static bool DirectoryExists(
            this IDirectoryAccessor directoryAccessor,
            string relativePath) =>
            directoryAccessor.DirectoryExists(new RelativeDirectoryPath(relativePath));

        public static void EnsureDirectoryExists(
            this IDirectoryAccessor directoryAccessor,
            string relativePath) =>
            directoryAccessor.EnsureDirectoryExists(new RelativeDirectoryPath(relativePath));

        public static bool FileExists(
            this IDirectoryAccessor directoryAccessor,
            string relativePath) =>
            directoryAccessor.FileExists(new RelativeFilePath(relativePath));

        public static string ReadAllText(
            this IDirectoryAccessor directoryAccessor,
            string relativePath) =>
            directoryAccessor.ReadAllText(new RelativeFilePath(relativePath));

        public static void WriteAllText(
            this IDirectoryAccessor directoryAccessor,
            string relativePath,
            string text) =>
            directoryAccessor.WriteAllText(
                new RelativeFilePath(relativePath),
                text);

        public static IDirectoryAccessor GetDirectoryAccessorForRelativePath(
            this IDirectoryAccessor directoryAccessor,
            string relativePath) =>
            directoryAccessor.GetDirectoryAccessorForRelativePath(new RelativeDirectoryPath(relativePath));

        public static DirectoryInfo GetFullyQualifiedRoot(this IDirectoryAccessor directoryAccessor) =>
            (DirectoryInfo) directoryAccessor.GetFullyQualifiedPath(new RelativeDirectoryPath("."));
    }
}