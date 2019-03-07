using System;
using System.IO;

namespace MLS.Agent.Tools.Extensions
{
    public static class StringExtensions
    {
        public static void Delete(this string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path);
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                throw new ArgumentException($"Couldn't find a file or directory called {path}");
            }
        }
    }
}
