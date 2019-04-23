using System;
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

    public interface IHaveADirectoryAccessor : IPackage
    {
        IDirectoryAccessor Directory { get; }
    }

    public interface IMightSupportBlazor : IPackage
    {
        bool CanSupportBlazor { get; }
    }

    public interface ICreateAWorkspace : IPackage
    {
        Task<Workspace> CreateRoslynWorkspaceForRunAsync(Budget budget);
    }

    public static class PackageFinder
    {
        public static Task<T> Find<T>(
            this IPackageFinder finder, 
            string packageName, 
            Budget budget = null) 
            where T : IPackage =>
            finder.Find<T>(new PackageDescriptor(packageName));

        public static IPackageFinder Create(IPackage package)
        {
            return new AnonymousPackageFinder(package);
        }

        private class AnonymousPackageFinder : IPackageFinder
        {
            private readonly IPackage _package;

            public AnonymousPackageFinder(IPackage package)
            {
                _package = package ?? throw new ArgumentNullException(nameof(package));
            }

            public Task<T> Find<T>(PackageDescriptor descriptor) where T : IPackage
            {
                if (_package is T package)
                {
                    return Task.FromResult(package);
                }

                return default;
            }
        }
    }

    
}