using System.Threading.Tasks;
using WorkspaceServer.Packaging;

namespace WorkspaceServer.Tests
{
    public static class Default
    {
        private static readonly PackageRegistry DefaultPackages = PackageRegistry.CreateForHostedMode();

        public static async Task<Package> ConsoleWorkspace() => (Package) await DefaultPackages.Get("console");

        public static async Task<Package> WebApiWorkspace() => (Package) await DefaultPackages.Get("aspnet.webapi");

        public static async Task<Package> XunitWorkspace() => (Package) await DefaultPackages.Get("xunit");

        public static async Task<Package> NetstandardWorkspace() => (Package) await DefaultPackages.Get("blazor-console");
    }
}