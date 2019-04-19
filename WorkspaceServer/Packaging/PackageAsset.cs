using System;
using System.Collections.Generic;

namespace WorkspaceServer.Packaging
{
    public class Package2 :
        IPackage
    {
        private readonly PackageDescriptor _descriptor;
        private readonly Dictionary<Type, PackageAsset> _assets = new Dictionary<Type, PackageAsset>();

        public Package2(
            string name,
            IDirectoryAccessor directoryAccessor)
        {
            _descriptor = new PackageDescriptor(name);

            DirectoryAccessor = directoryAccessor;
        }

        public Package2(
            PackageDescriptor descriptor,
            IDirectoryAccessor directoryAccessor)
        {
            _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));

            DirectoryAccessor = directoryAccessor ?? throw new ArgumentNullException(nameof(directoryAccessor));
        }

        public IEnumerable<PackageAsset> Assets => _assets.Values; 

        public string Name => _descriptor.Name;

        public string Version => _descriptor.Version;

        protected IDirectoryAccessor DirectoryAccessor { get; }

        public void Add(PackageAsset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            var packageRoot = DirectoryAccessor.GetFullyQualifiedRoot().FullName;
            var assetRoot = asset.DirectoryAccessor.GetFullyQualifiedRoot().FullName;

            if (!packageRoot.Contains(assetRoot))
            {
                throw new ArgumentException("Asset must be located under package path");
            }

            _assets.Add(asset.GetType(), asset);
        }
    }

    public abstract class PackageAsset
    {
        protected PackageAsset(IDirectoryAccessor directoryAccessor)
        {
            DirectoryAccessor = directoryAccessor ?? throw new ArgumentNullException(nameof(directoryAccessor));
        }

        protected internal IDirectoryAccessor DirectoryAccessor { get; }
    }

    public class ProjectAsset : PackageAsset
    {
        public ProjectAsset(IDirectoryAccessor directoryAccessor) : base(directoryAccessor)
        {
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
}