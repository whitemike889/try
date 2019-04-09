using System;
using System.IO;
using Clockwise;
using System.Linq;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using Pocket;
using static Pocket.Logger<WorkspaceServer.Packaging.BuildableArtifact>;

namespace WorkspaceServer.Packaging
{
    public abstract class BuildableArtifact
    {
        internal const string FullBuildBinlogFileName = "package_fullBuild.binlog";

        private static readonly object _lockObj = new object();
        private readonly AsyncLazy<bool> _lazyCreation;

        protected BuildableArtifact(
            string name = null,
            IPackageInitializer initializer = null,
            DirectoryInfo directory = null)
        {
            Initializer = initializer;
            Directory = directory ?? new DirectoryInfo(Path.Combine(Package.DefaultPackagesDirectory.FullName, Name));

            Name = name ?? directory?.Name ?? throw new ArgumentException($"You must specify {nameof(name)}, {nameof(directory)}, or both.");

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

        protected internal virtual async Task EnsureReady(Budget budget)
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

        public virtual async Task FullBuild()
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                await DotnetBuild();
                operation.Succeed();
            }
        }

        protected async Task DotnetBuild()
        {
            var projectFile = GetProjectFile();
            var args = $"/bl:{FullBuildBinlogFileName}";
            if (projectFile?.Exists == true)
            {
                args = $"{projectFile.FullName} {args}";
            }

            var result = await new Dotnet(Directory)
                             .Build(args: args);
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
        }

        protected FileInfo LastBuildErrorLogFile { get; }

        protected FileInfo GetProjectFile()
        {
            return Directory.GetFiles("*.csproj").FirstOrDefault();
        }

        public static DirectoryInfo CreateDirectory(
            string folderNameStartsWith,
            DirectoryInfo parentDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(folderNameStartsWith))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(folderNameStartsWith));
            }

            parentDirectory = parentDirectory ?? Package.DefaultPackagesDirectory;

            DirectoryInfo created;

            lock (_lockObj)
            {
                var existingFolders = parentDirectory.GetDirectories($"{folderNameStartsWith}.*");

                created = parentDirectory.CreateSubdirectory($"{folderNameStartsWith}.{existingFolders.Length + 1}");
            }

            return created;
        }
    }
}