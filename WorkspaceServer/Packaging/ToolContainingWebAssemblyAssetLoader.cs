using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkspaceServer.Packaging
{
    internal class ToolContainingWebAssemblyAssetLoader : IPackageAssetLoader
    {
      
        public Task<IEnumerable<PackageAsset>> LoadAsync(Package2 package)
        {
            var directory = package.DirectoryAccessor;

            if (directory.DirectoryExists(".store") )
            {
                var exeName = package.Name.ExecutableName();

                if (directory.FileExists(exeName)) 
                {

                }
            }

            return Task.FromResult(Enumerable.Empty<PackageAsset>());
        }
    }
}