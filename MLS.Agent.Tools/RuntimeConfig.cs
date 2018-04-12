using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace MLS.Agent.Tools
{
    public static class RuntimeConfig
    {
        public static string GetTargetFramework(FileInfo runtimeConfigFile)
        {
            if (runtimeConfigFile == null)
            {
                throw new ArgumentNullException(nameof(runtimeConfigFile));
            }

            var content = runtimeConfigFile.Read();

            var fileContentJson = JObject.Parse(content);

            return fileContentJson["runtimeOptions"]["tfm"].Value<string>();
        }
    }
}
