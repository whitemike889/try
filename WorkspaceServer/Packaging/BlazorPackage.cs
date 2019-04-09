using System;
using System.IO;

namespace WorkspaceServer.Packaging
{
    public class BlazorPackage : BuildableArtifact
    {
        private const string runnerPrefix = "runner-";
        private FileInfo _blazorEntryPoint;

        public BlazorPackage(
            string name,
            IPackageInitializer initializer = null,
            DirectoryInfo directory = null) : base(name, initializer, directory)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }

            if (name.StartsWith(runnerPrefix))
            {
                CodeRunnerPath = $"/LocalCodeRunner/{name.Remove(0, runnerPrefix.Length)}";
                CodeRunnerPathBase = CodeRunnerPath + "/";
            }
            else
            {
                throw new ArgumentException(nameof(name));
            }
        }

        public FileInfo BlazorEntryPointAssemblyPath =>
            _blazorEntryPoint ?? (_blazorEntryPoint = GetBlazorEntryPoint());

        public string CodeRunnerPath { get; }

        public string CodeRunnerPathBase { get; }

        private FileInfo GetBlazorEntryPoint()
        {
            var path = Path.Combine(Directory.FullName, "runtime", "MLS.Blazor.dll");
            return new FileInfo(path);
        }
    }
}