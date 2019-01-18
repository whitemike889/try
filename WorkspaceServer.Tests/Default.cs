using System.Threading.Tasks;
using WorkspaceServer.Packaging;

namespace WorkspaceServer.Tests
{
    public static class Default
    {
        private static readonly PackageRegistry DefaultPackages = PackageRegistry.CreateForHostedMode();

        public static Task<Package> ConsoleWorkspace => DefaultPackages.Get("console");

        public static Task<Package> WebApiWorkspace => DefaultPackages.Get("aspnet.webapi");

        public static Task<Package> XunitWorkspace => DefaultPackages.Get("xunit");
        public static Task<Package> NetstandardWorkspace => DefaultPackages.Get("blazor-console");
    }
}
