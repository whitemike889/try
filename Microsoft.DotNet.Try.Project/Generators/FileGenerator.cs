using MLS.Protocol.Execution;

namespace Microsoft.DotNet.Try.Project.Generators
{
    public static class FileGenerator
    {
        public static Workspace.File Create(string name, string content)
        {
            return new Workspace.File(name, CodeManipulation.EnforceLF(content));
        }
    }
}