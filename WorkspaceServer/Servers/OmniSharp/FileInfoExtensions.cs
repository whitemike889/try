using System.IO;
using System.Threading.Tasks;

namespace WorkspaceServer.Servers.OmniSharp
{
    public static class FileInfoExtensions
    {
        public static async Task<string> ReadAsync(this FileInfo file)
        {
            using (var reader = file.OpenText())
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}