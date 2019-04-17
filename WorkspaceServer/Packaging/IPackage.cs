using System.IO;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;

namespace WorkspaceServer.Packaging
{
    public interface IPackage
    {
        string Name { get; }
    }

    public interface IHaveADirectory : IPackage
    {
        DirectoryInfo Directory { get; }
    }

    public interface IHaveAName : IPackage
    {
        string Name { get; }
    }

    public interface IMayOrMayNotSupportBlazor : IPackage
    {
        bool CanSupportBlazor { get; }
    }

    public interface ICreateAWorkspace : IPackage
    {
        Task<Workspace> CreateRoslynWorkspaceForRunAsync(Budget budget);
    }
}