using Microsoft.DotNet.Try.Protocol;

namespace Microsoft.DotNet.Try.Project
{
    public static class FileGenerator
    {
        public static Workspace.File Create(string name, string content)
        {
            return new Workspace.File(name, content.EnforceLF());
        }
    }
}