using Clockwise;
using System.Threading.Tasks;
using WorkspaceServer.Packaging;

namespace WorkspaceServer
{
    public interface IPackageDiscoveryStrategy
    {
        Task<PackageBuilder> Locate(string workspaceName, Budget budget);
    }
}