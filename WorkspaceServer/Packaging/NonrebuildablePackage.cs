using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using MLS.Agent.Tools;

namespace WorkspaceServer.Packaging
{
    public class NonrebuildablePackage : Package
    {
        private readonly AsyncLazy<bool> _built;
        private readonly AsyncLazy<bool> _published;
        private readonly Lazy<SyntaxTree> _instrumentationEmitterSyntaxTree;
        private bool _isReady;

        public NonrebuildablePackage(string name = null, IPackageInitializer initializer = null, DirectoryInfo directory = null, IScheduler buildThrottleScheduler = null) 
            : base(name, initializer, directory, buildThrottleScheduler)
        {
            _built = new AsyncLazy<bool>(base.EnsureBuilt);
            _published = new AsyncLazy<bool>(base.EnsurePublished);
            _instrumentationEmitterSyntaxTree = new Lazy<SyntaxTree>(CreateInstrumentationEmitterSyntaxTree);
        }

        public override SyntaxTree GetInstrumentationEmitterSyntaxTree() => _instrumentationEmitterSyntaxTree.Value;

        public async override Task EnsureReady(Budget budget)
        {
            if (_isReady)
            {
                return;
            }

            await base.EnsureReady(budget);

            _isReady = true;
        }

        protected async override Task<bool> EnsureBuilt() => await _built.ValueAsync();

        public async override Task<bool> EnsurePublished() => await _published.ValueAsync();
    }
}
