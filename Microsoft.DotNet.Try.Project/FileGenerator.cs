using Microsoft.DotNet.Try.Protocol;

namespace Microsoft.DotNet.Try.Project
{
    public static class FileGenerator
    {
        public static File Create(string name, string content)
        {
            return new File(name, content);
        }
    }
}