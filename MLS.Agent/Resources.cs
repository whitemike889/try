using System.IO;

namespace MLS.Agent
{
    public static class Resources
    {
        public static string ReadManifestResource(string resourceName)
        {
            var assembly = typeof(Program).Assembly;
            using (var reader = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
