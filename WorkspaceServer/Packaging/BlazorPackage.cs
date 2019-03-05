using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using Clockwise;

namespace WorkspaceServer.Packaging
{
    class BlazorPackage : Package
    {
        public BlazorPackage(string name = null, IPackageInitializer initializer = null, DirectoryInfo directory = null, IScheduler buildThrottleScheduler = null) : base(name, initializer, directory, buildThrottleScheduler)
        {
        }

        protected override Task<bool> EnsureBuilt()
        {
            return Task.FromResult(true);
        }

        public override Task<bool> EnsurePublished()
        {
            return Task.FromResult(true);
        }

        protected override bool ShouldBuild()
        {
            return false;
        }
    }
}
