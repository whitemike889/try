using Microsoft.DotNet.Try.Markdown;
using MLS.Agent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WorkspaceServer.Packaging
{
    public class PackageAsset
    {
        private IDirectoryAccessor _directoryAccessor;

        public PackageAsset(IDirectoryAccessor directoryAccessor)
        {
            _directoryAccessor = directoryAccessor ?? throw new System.ArgumentNullException(nameof(directoryAccessor));
            Directory = (DirectoryInfo)_directoryAccessor.GetFullyQualifiedPath(new RelativeDirectoryPath("."));
        }

        public DirectoryInfo Directory { get; set; }

        public IEnumerable<FileInfo> GetFiles()
        {
            return _directoryAccessor.GetAllFilesRecursively()
                .Select(relativePath => (FileInfo) _directoryAccessor.GetFullyQualifiedPath(relativePath));
        }
    }
}