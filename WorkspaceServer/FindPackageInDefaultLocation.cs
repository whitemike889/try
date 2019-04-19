using System.Threading.Tasks;
using WorkspaceServer.Packaging;

namespace WorkspaceServer
{
    public class FindPackageInDefaultLocation : IPackageFinder
    {
        private readonly IDirectoryAccessor _directoryAccessor;

        public FindPackageInDefaultLocation(IDirectoryAccessor directoryAccessor)
        {
            _directoryAccessor = directoryAccessor;
        }

        public async Task<T> Find<T>(PackageDescriptor descriptor) where T : IPackage
        {
            Package2 package = null;


            // if (_directoryAccessor.DirectoryExists(descriptor.Name))
            // {
            //     package = new Package2(
            //         descriptor.Name,
            //         descriptor.Version,
            //         DirectoryAccessor.
            //     );
            // }

            return default;
        }
    }
}