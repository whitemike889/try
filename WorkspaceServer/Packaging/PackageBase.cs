using System;
using System.IO;
using Clockwise;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using Pocket;
using static Pocket.Logger<WorkspaceServer.Packaging.PackageBase>;

namespace WorkspaceServer.Packaging
{
    public abstract class PackageBase : 
        IPackage, 
        IHaveADirectory,
        IMightSupportBlazor
        , IHaveADirectoryAccessor
    {
        IDirectoryAccessor IHaveADirectoryAccessor.Directory => new FileSystemDirectoryAccessor(Directory);

        internal const string FullBuildBinlogFileName = "package_fullBuild.binlog";

        private readonly AsyncLazy<bool> _lazyCreation;
        private bool? _canSupportBlazor;

        protected PackageBase(
            string name = null,
            IPackageInitializer initializer = null,
            DirectoryInfo directory = null)
        {
            Initializer = initializer;

            Name = name ?? directory?.Name ?? throw new ArgumentException($"You must specify {nameof(name)}, {nameof(directory)}, or both.");
            Directory = directory ?? new DirectoryInfo(Path.Combine(Package.DefaultPackagesDirectory.FullName, Name));

            _lazyCreation = new AsyncLazy<bool>(Create);
            LastBuildErrorLogFile = new FileInfo(Path.Combine(Directory.FullName, ".trydotnet-builderror"));
        }

        public IPackageInitializer Initializer { get; protected set; }

        public string Name { get; }

        public DirectoryInfo Directory { get; set; }

        protected bool IsDirectoryCreated { get; set; }

        protected Task<bool> EnsureCreated() => _lazyCreation.ValueAsync();

        protected virtual async Task EnsureBuilt([CallerMemberName] string caller = null)
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                await EnsureCreated();

                await FullBuild();

                operation.Succeed();
            }
        }

        public virtual async Task EnsureReady(Budget budget)
        {
            await EnsureCreated().CancelIfExceeds(budget);

            await EnsureBuilt().CancelIfExceeds(budget);

            budget.RecordEntry();
        }

        protected async Task<bool> Create()
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                //to do: identify if this flag is needed
                if (!IsDirectoryCreated)
                {
                    Directory.Refresh();

                    if (!Directory.Exists)
                    {
                        operation.Info("Creating directory {directory}", Directory);
                        Directory.Create();
                        Directory.Refresh();
                    }

                    IsDirectoryCreated = true;
                }

                if (Directory.GetFiles("*", SearchOption.AllDirectories).Length == 0)
                {
                    operation.Info("Initializing package using {_initializer} in {directory}", Initializer, Directory);
                    await Initializer.Initialize(Directory);
                }

                operation.Succeed();
                return true;
            }
        }

        public bool CanSupportBlazor
        {
            get
            {
                // The directory structure for the blazor packages is as follows
                // project |--> packTarget
                //         |--> runner-abc 
                // The packTarget is the project that contains this packaga
                //Hence the parent directory must be looked for the blazor runner
                if (_canSupportBlazor == null)
                {
                    _canSupportBlazor = Directory?.Parent?.GetDirectories($"runner-{Name}")?.Length == 1;
                }

                return _canSupportBlazor.Value;
            }
        }

        public virtual async Task FullBuild()
        {
            await DotnetBuild();
        }

        protected async Task DotnetBuild()
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                var projectFile = GetProjectFile();
                var args = $"/bl:{FullBuildBinlogFileName}";
                if (projectFile?.Exists == true)
                {
                    args = $"{projectFile.FullName} {args}";
                }

                operation.Info("Building package {name} in {directory}", Name, Directory);

                var result = await new Dotnet(Directory).Build(args: args);

                if (result.ExitCode != 0)
                {
                    File.WriteAllText(
                        LastBuildErrorLogFile.FullName,
                        string.Join(Environment.NewLine, result.Error));
                }
                else if (LastBuildErrorLogFile.Exists)
                {
                    LastBuildErrorLogFile.Delete();
                }

                result.ThrowOnFailure();
                operation.Succeed();
            }
        }

        protected FileInfo LastBuildErrorLogFile { get; }

        protected FileInfo GetProjectFile()
        {
            return Directory.GetFiles("*.csproj").FirstOrDefault();
        }

    }
}