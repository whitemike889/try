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
        private FileInfo _blazorEntryPoint;

        public BlazorPackage(string name = null, IPackageInitializer initializer = null, DirectoryInfo directory = null, IScheduler buildThrottleScheduler = null) : base(name, initializer, directory, buildThrottleScheduler)
        {
        }

        protected override bool ShouldDoDesignTimeFullBuild()
        {
            return false;
        }

        public FileInfo BlazorEntryPointAssemblyPath =>
            _blazorEntryPoint ?? (_blazorEntryPoint = GetBlazorEntryPoint());

        private FileInfo GetBlazorEntryPoint()
        {
            return Directory.GetFiles("MLS.Blazor.dll", SearchOption.AllDirectories).First();
        }
    }
}
