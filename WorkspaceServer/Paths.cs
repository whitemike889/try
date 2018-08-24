using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WorkspaceServer
{
    public static class Paths
    {
        static Paths()
        {
            UserProfile = Environment.GetEnvironmentVariable(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "USERPROFILE"
                    : "HOME");

            var dotnetToolsPath = Environment.GetEnvironmentVariable("DOTNET_TOOLS");

            DotnetToolsPath = string.IsNullOrWhiteSpace(dotnetToolsPath)
                                  ? Path.Combine(UserProfile, ".dotnet", "tools")
                                  : dotnetToolsPath;

            var nugetPackagesEnvironmentVariable = Environment.GetEnvironmentVariable("NUGET_PACKAGES");

            NugetCache = string.IsNullOrWhiteSpace(nugetPackagesEnvironmentVariable)
                             ? Path.Combine(UserProfile, ".nuget", "packages")
                             : nugetPackagesEnvironmentVariable;
        }

        public static string DotnetToolsPath { get; }

        public static string UserProfile { get; }

        public static string NugetCache { get; }

        public static string ExecutableName(this string withoutExtension) =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? withoutExtension + ".exe"
                : withoutExtension;
    }
}
