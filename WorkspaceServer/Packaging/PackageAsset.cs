using System;
using System.IO;

namespace WorkspaceServer.Packaging
{
    public abstract class PackageAsset
    {
        protected PackageAsset(IDirectoryAccessor directoryAccessor)
        {
            DirectoryAccessor = directoryAccessor ?? throw new ArgumentNullException(nameof(directoryAccessor));
        }

        public IDirectoryAccessor DirectoryAccessor { get; }
    }

    public class ProjectAsset : PackageAsset
    {
        private readonly RebuildablePackage internalPackage;

        public ProjectAsset(IDirectoryAccessor directoryAccessor) : base(directoryAccessor)
        {
            internalPackage = new RebuildablePackage(directory: DirectoryAccessor.GetFullyQualifiedRoot());
        }

        public FileInfo EntryPoint => internalPackage.EntryPointAssemblyPath;
    }

    public class WebAssemblyAsset : PackageAsset
    {
        public WebAssemblyAsset(IDirectoryAccessor directoryAccessor) : base(directoryAccessor)
        {
        }
    }

    public class ContentAsset : PackageAsset
    {
        public ContentAsset(IDirectoryAccessor directoryAccessor) : base(directoryAccessor)
        {
        }
    }
}