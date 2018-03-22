using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MLS.Agent.Tools
{
    public static class Paths
    {
        static Paths()
        {
            UserProfile = Environment.GetEnvironmentVariable(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "USERPROFILE"
                    : "HOME");

            var nugetPackagesEnvironmentVariable = Environment.GetEnvironmentVariable("NUGET_PACKAGES");

            NugetCache = string.IsNullOrWhiteSpace(nugetPackagesEnvironmentVariable)
                             ? Path.Combine(UserProfile, ".nuget", "packages")
                             : nugetPackagesEnvironmentVariable;

            EmitPlugin = Path.Combine(NugetCache, "trydotnet.omnisharp.emit", "1.29.0-beta2", "lib", "net46", "OmniSharp.Emit.dll");
        }

        public static string UserProfile { get; }

        public static string NugetCache { get; }

        public static string EmitPlugin { get; }
    }
}
