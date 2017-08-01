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
                AssemblyInformationalVersion = assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion,
                AssemblyVersion = assembly.GetName().Version.ToString(),
                BuildDate = new FileInfo(new Uri(assembly.CodeBase).LocalPath).CreationTimeUtc.ToString("o")
            };

            return info;
        });

        public static BuildInfo Version()
        {
            return buildInfo.Value;
        }

        public class BuildInfo
        {
            public string AssemblyVersion { get; set; }
            public string BuildDate { get; set; }
            public string AssemblyInformationalVersion { get; set; }
            public string AssemblyName { get; set; }
        }
    }
}
