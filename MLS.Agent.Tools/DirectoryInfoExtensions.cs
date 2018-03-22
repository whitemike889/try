using System;
using System.IO;

namespace MLS.Agent.Tools
{
    public static class DirectoryInfoExtensions
    {
        public static void CopyTo(
            this DirectoryInfo source,
            DirectoryInfo destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (!source.Exists)
            {
                throw new DirectoryNotFoundException(source.FullName);
            }

            if (!destination.Exists)
            {
                destination.Create();
            }

            foreach (var file in source.GetFiles())
            {
                file.CopyTo(
                    Path.Combine(
                        destination.FullName, file.Name), false);
            }

            foreach (var subdirectory in source.GetDirectories())
            {
                subdirectory.CopyTo(
                    new DirectoryInfo(
                        Path.Combine(
                            destination.FullName, subdirectory.Name)));
            }
        }
    }
}
