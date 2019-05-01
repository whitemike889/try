using System.IO;
using System.Threading.Tasks;
using WorkspaceServer.Packaging;

namespace WorkspaceServer
{
    public class PackageNameIsFullyQualifiedPath : IPackageFinder
    {
        public Task<T> Find<T>(PackageDescriptor descriptor)
            where T : IPackage
        {
            if (descriptor.IsPathSpecified)
            {
                var pkg = new Package2(descriptor.Name, new FileSystemDirectoryAccessor(new FileInfo(descriptor.Name).Directory));

                if (pkg is T t)
                {
                    return Task.FromResult(t);
                }
            }

            return Task.FromResult<T>(default);
        }
    }
}