using System.Collections.Generic;

namespace WorkspaceServer.Packaging
{
    public class PackageConfiguration
    {
        public IReadOnlyCollection<string> CompilerArgs { get; set; }
    }
}