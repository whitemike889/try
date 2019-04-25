using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkspaceServer.Packaging
{
    internal class ProjectAssetLoader : IPackageAssetLoader
    {
        public Task<IEnumerable<PackageAsset>> LoadAsync(Package2 package)
        {
            var assets = new List<PackageAsset>();

            var directory = package.DirectoryAccessor;

            foreach (var csproj in directory.GetAllFilesRecursively()
                                            .Where(f => f.Extension == ".csproj"))
            {
                assets.Add(new ProjectAsset(directory.GetDirectoryAccessorForRelativePath(csproj.Directory)));
            }

            return
                Task.FromResult<IEnumerable<PackageAsset>>(assets);
        }
    }
}