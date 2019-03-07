using System;
using System.IO;

namespace MLS.Agent.Tools.Extensions
{
    public static class TypeExtensions
    {
        public static string ReadManifestResource(this Type type, string resourceName)
        {
            var assembly = type.Assembly;
            using (var reader = new StreamReader(assembly.GetManifestResourceStream($"WorkspaceServer.{resourceName}")))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
