using System.IO;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MLS.Agent.Tools;

namespace WorkspaceServer.Packaging
{
    public class NonrebuildablePackage : Package
    {
        private readonly AsyncLazy<CSharpCommandLineArguments> _csharpCommandLineArguments;
        private readonly AsyncLazy<bool> _created;
        private readonly AsyncLazy<bool> _built;
        private readonly AsyncLazy<bool> _published;
        private readonly AsyncLazy<SyntaxTree> _instrumentationEmitterSyntaxTree;
        private bool _isReady;

        public NonrebuildablePackage(string name = null, IPackageInitializer initializer = null, DirectoryInfo directory = null) : base(name, initializer, directory)
        {
            _csharpCommandLineArguments = new AsyncLazy<CSharpCommandLineArguments>(CreateCSharpCommandLineArguments);
            _created = new AsyncLazy<bool>(base.EnsureCreated);
            _built = new AsyncLazy<bool>(base.EnsureBuilt);
            _published = new AsyncLazy<bool>(base.EnsurePublished);
            _instrumentationEmitterSyntaxTree = new AsyncLazy<SyntaxTree>(CreateInstrumentationEmitterSyntaxTree);
        }

        public override Task<CSharpCommandLineArguments> GetCommandLineArguments() => _csharpCommandLineArguments.ValueAsync();

        public override Task<SyntaxTree> GetInstrumentationEmitterSyntaxTree() => _instrumentationEmitterSyntaxTree.ValueAsync();

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

        protected async override Task<bool> EnsureCreated() => await _created.ValueAsync();

        public async override Task<bool> EnsurePublished() => await _published.ValueAsync();
    }
}
