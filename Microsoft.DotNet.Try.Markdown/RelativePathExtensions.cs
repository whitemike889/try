using System.IO;

namespace Microsoft.DotNet.Try.Markdown
{
    public static class RelativePathExtensions
    {
        public static FileInfo Combine(
            this DirectoryInfo directory,
            RelativeFilePath filePath)
        {
            var filePart = filePath.Value;

            if (filePart.StartsWith("./"))
            {
                filePart = filePart.Substring(2);
            }

            return new FileInfo(
                Path.Combine(
                    directory.FullName,
                    filePart.Replace('/', Path.DirectorySeparatorChar)));
        }

        public static DirectoryInfo Combine(
            this DirectoryInfo directory,
            RelativeDirectoryPath directoryPath)
        {
            return new DirectoryInfo(
                Path.Combine(
                    RelativePath.NormalizeDirectory(directory.FullName),
                    directoryPath.Value.Replace('/', Path.DirectorySeparatorChar)));
        }
    }
}