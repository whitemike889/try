using System.IO;
using System.Threading.Tasks;

namespace MLS.Agent.Tools
{
    public static class FileInfoExtensions
    {
        public static string Read(this FileInfo file)
        {
            using (var reader = file.OpenText())
            {
                return reader.ReadToEnd();
            }
        }

        public static async Task<string> ReadAsync(this FileInfo file)
        {
            using (var reader = file.OpenText())
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
