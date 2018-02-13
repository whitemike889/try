using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MLS.Agent.Tools
{
    public static class Paths
    {
        public static string UserProfile { get; } =
            Environment.GetEnvironmentVariable(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "USERPROFILE"
                    : "HOME");

        public static string NugetCache { get; } =
            Path.Combine(UserProfile, ".nuget", "packages");

        public static string EmitPlugin { get; } = Path.Combine(NugetCache, "trydotnet.omnisharp.emit", "1.29.0-beta2", "lib", "net46", "OmniSharp.Emit.dll");
    }
}
