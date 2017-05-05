using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Recipes
{
#if !RecipesProject
    [DebuggerStepThrough]
#endif
    internal class AssemblyVersionSensor
    {
        private static readonly Lazy<BuildInfo> buildInfo = new Lazy<BuildInfo>(() =>
        {
            var assembly = typeof(AssemblyVersionSensor).GetTypeInfo().Assembly;

            var info = new BuildInfo
            {
                AssemblyName = assembly.GetName().Name,
                AssemblyInformationalVersion = Assembly.GetEntryAssembly()
                                                       .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                                       .InformationalVersion,
                BuildVersion = assembly.GetName().Version.ToString(),
                BuildDate = new FileInfo(new Uri(assembly.CodeBase).LocalPath).CreationTimeUtc.ToString("o")
            };

            return info;
        });

        public static dynamic Version()
        {
            return new
            {
                buildInfo.Value.AssemblyName,
                buildInfo.Value.AssemblyInformationalVersion,
                buildInfo.Value.BuildVersion,
                buildInfo.Value.BuildDate
            };
        }

        private class BuildInfo
        {
            public string BuildVersion;
            public string BuildDate;
            public string AssemblyInformationalVersion { get; set; }
            public string AssemblyName { get; set; }
        }
    }
}
