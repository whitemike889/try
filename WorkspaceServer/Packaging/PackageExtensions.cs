using System.IO;
using System.Threading.Tasks;
using Pocket;

namespace WorkspaceServer.Packaging
{
    internal static class PackageExtensions
    {
        public static async Task<bool> Create(
            this IHaveADirectory packageBase, 
            IPackageInitializer initializer)
        {
            using (var operation = Logger<PackageBase>.Log.OnEnterAndConfirmOnExit())
            {
                if (!packageBase.Directory.Exists)
                {

                    operation.Info("Creating directory {directory}", packageBase.Directory);
                    packageBase.Directory.Create();
                    packageBase.Directory.Refresh();
                }

                if (packageBase.Directory.GetFiles("*", SearchOption.AllDirectories).Length == 0)
                {
                    operation.Info("Initializing package using {_initializer} in {directory}", initializer, packageBase.Directory);
                    await initializer.Initialize(packageBase.Directory);
                }

                operation.Succeed();
                return true;
            }
        }
    }
}