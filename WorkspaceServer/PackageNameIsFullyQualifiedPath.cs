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

                // FIX: (Find) don't assume the csproj defines the package root
                var pkg = new Package2(descriptor.Name, new FileSystemDirectoryAccessor(new FileInfo(descriptor.Name).Directory));

                if (pkg is T t)
                {
                    return t;
                }
            }

            return default;
        }
    }
}