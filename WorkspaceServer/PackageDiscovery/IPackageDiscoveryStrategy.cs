using Clockwise;
using System.IO;
using System.Threading.Tasks;
using WorkspaceServer.Packaging;

namespace WorkspaceServer
{
    public interface IPackageDiscoveryStrategy
    {
        Task<PackageBuilder> Locate(PackageDescriptor packageInfo, Budget budget = null);
    }
}