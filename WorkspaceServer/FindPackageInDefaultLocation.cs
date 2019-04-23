using System.IO;
using System.Threading.Tasks;
using WorkspaceServer.Packaging;

namespace WorkspaceServer
{
    public class PackageNameIsFullyQualifiedPath : IPackageFinder
    {
        public async Task<T> Find<T>(PackageDescriptor descriptor)
            where T : IPackage
        {
            if (descriptor.IsPathSpecified)
            {
                var pkg = new Package2(descriptor.Name, new FileSystemDirectoryAccessor(new FileInfo(descriptor.Name).Directory));

                if (pkg is T t)
                {
                    return t;
                }
            }

            return default;
        }
    }

    public class FindPackageInDefaultLocation : IPackageFinder
    {
        private readonly IDirectoryAccessor _directoryAccessor;

        public FindPackageInDefaultLocation(IDirectoryAccessor directoryAccessor = null)
        {
            _directoryAccessor = directoryAccessor ??
                                 new FileSystemDirectoryAccessor(Package.DefaultPackagesDirectory);
        }

        public async Task<T> Find<T>(PackageDescriptor descriptor)
            where T : IPackage
        {
            if (!descriptor.IsPathSpecified &&
                _directoryAccessor.DirectoryExists(descriptor.Name))
            {
                var pkg = new Package2(
                    descriptor,
                    _directoryAccessor);

                if (pkg is T t)
                {
                    return t;
                }
            }

            return default;
        }
    }
}