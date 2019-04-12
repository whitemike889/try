using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace WorkspaceServer.Packaging
{
    internal static class RuntimeConfig
    {
        public static string GetTargetFramework(FileInfo runtimeConfigFile)
        {
            if (runtimeConfigFile == null)
            {
                throw new ArgumentNullException(nameof(runtimeConfigFile));
            }

            var content = File.ReadAllText(runtimeConfigFile.FullName);

            var fileContentJson = JObject.Parse(content);

            return fileContentJson["runtimeOptions"]["tfm"].Value<string>();
        }
    }
}
