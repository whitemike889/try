using System.Threading.Tasks;
using WorkspaceServer.Packaging;

namespace WorkspaceServer
{
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
            var package = default(T);

            if (_directoryAccessor.DirectoryExists(descriptor.Name))
            {
                var pkg = new Package2(
                    descriptor,
                    _directoryAccessor);

                if (pkg is T t)
                {
                    return t;
                }
            }

            return package;
        }
    }
}