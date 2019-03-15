using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;

namespace WorkspaceServer.Packaging
{
    public class BlazorPackage : Package
    {
        private const string runnerPrefix = "runner-";
        private FileInfo _blazorEntryPoint;

        public BlazorPackage(string name = null, IPackageInitializer initializer = null, DirectoryInfo directory = null, IScheduler buildThrottleScheduler = null) : base(name, initializer, directory, buildThrottleScheduler)
        {
            if (!name.StartsWith(runnerPrefix))
            {
                throw new ArgumentException(nameof(name));
            }

            CodeRunnerPath = $"/LocalCodeRunner/{name.Remove(0, runnerPrefix.Length)}";
            CodeRunnerPathBase = CodeRunnerPath + "/";
        }

        protected override bool ShouldDoDesignTimeFullBuild()
        {
            return false;
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

        public async Task Prepare()
        {
            await base.EnsureBuilt();
        }
    }
}
