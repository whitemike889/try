using System;
using System.Runtime.InteropServices;

namespace MLS.Agent.Tools
{
    internal static class Paths
    {
        public static string UserProfile() =>
            Environment.GetEnvironmentVariable(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "USERPROFILE"
                    : "HOME");
    }
}