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
        }

        public static string UserProfile { get; }

        public static string NugetCache { get; }
    }
}
