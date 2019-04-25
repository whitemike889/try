using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkspaceServer.Packaging
{
    public interface IPackageAssetLoader
    {
        Task<IEnumerable<PackageAsset>> LoadAsync(Package2 package);
    }
}