using System;
using System.Collections;
using System.Collections.Generic;

namespace WorkspaceServer.Packaging
{
    public class Package2 : IEnumerable<PackageAsset>, IPackage
    {
        private readonly Dictionary<Type, PackageAsset> _assets = new Dictionary<Type, PackageAsset>();

        public Package2(
            string name,
            IDirectoryAccessor directoryAccessor)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            DirectoryAccessor = directoryAccessor;
        }

        public string Name { get; }

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

        public IEnumerator<PackageAsset> GetEnumerator()
        {
            return _assets.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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