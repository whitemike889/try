using System.Threading.Tasks;
using Clockwise;

namespace WorkspaceServer.Packaging
{
    public interface IPackageDiscoveryStrategy
    {
        Task<PackageBuilder> Locate(PackageDescriptor packageInfo, Budget budget = null);
    }
}