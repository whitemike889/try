using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using WorkspaceServer.Tests;

namespace MLS.Agent.Tests
{
    internal static class DirectoryInfoExtensions
    {
        public static string ToJsonStructure(this DirectoryInfo source)
        {
            var report =  new JObject
            {
                {"name", source.Name }
            };

            var rootPath = source.FullName;

            var content = new JArray( source
                .GetFiles("*", SearchOption.AllDirectories)
                .OrderBy(f => f.Directory?.FullName.Split(Environment.NewLine).Length ?? 0)
                .ThenBy(f => f.FullName)
                .Select(f => f.ToJsonStructure(rootPath)));

            report["content"] = content;
            return report.ToString().FormatJson();
        }

        private static JToken ToJsonStructure(this FileSystemInfo source, string root)
        {
            return new JObject
            {
                {"name", source.FullName.Replace(root, string.Empty).Replace("\\", "/") }
            };
        }
    }
}