using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MLS.Agent.Tools
{
    public static class Paths
    {
        public static string UserProfile() =>
            Environment.GetEnvironmentVariable(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "USERPROFILE"
                    : "HOME");

        public static string NugetCache() =>
            Path.Combine(UserProfile(), ".nuget", "packages");
    }
}