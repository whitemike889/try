using System.Threading.Tasks;

namespace WorkspaceServer.Packaging
{
    public interface IPackageFinder
    {
        Task<T> Find<T>(PackageDescriptor descriptor) where T : class, IPackage;
    }
}