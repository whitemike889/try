using System;
using System.IO;

namespace MLS.Agent.Tools
{
    public static class TypeExtensions
    {
        public static string ReadManifestResource(this Type type, string resourceName)
        {
            var assembly = type.Assembly;
            using (var reader = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
