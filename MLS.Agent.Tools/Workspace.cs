using System;
using System.IO;
using Clockwise;
using System.Linq;
using System.Threading.Tasks;
using Pocket;

namespace MLS.Agent.Tools
{
    public class Workspace
    {
        static Workspace()
        {
            var workspacesPathEnvironmentVariableName = "TRYDOTNET_WORKSPACES_PATH";

            var environmentVariable = Environment.GetEnvironmentVariable(workspacesPathEnvironmentVariableName);

            DefaultWorkspacesDirectory =
                environmentVariable != null
                    ? new DirectoryInfo(environmentVariable)
                    : new DirectoryInfo(
                        Path.Combine(
                            Paths.UserProfile,
                            ".trydotnet",
                            "workspaces"));

            if (!DefaultWorkspacesDirectory.Exists)
            {
                DefaultWorkspacesDirectory.Create();
            }

            Logger<Workspace>.Log.Info("Workspaces path is {DefaultWorkspacesDirectory}", DefaultWorkspacesDirectory);
        }

        private readonly IWorkspaceInitializer _initializer;
        private static readonly object _lockObj = new object();
        private readonly AsyncLazy<bool> _created;
        private readonly AsyncLazy<bool> _built;
        private readonly AsyncLazy<bool> _published;
        private bool? _isWebProject;
        private FileInfo _entryPointAssemblyPath;
        private static string _targetFramework;
        private readonly Logger _log;

        public DateTimeOffset? ConstructionTime { get; }
        public DateTimeOffset? CreationTime { get; private set; }
        public DateTimeOffset? BuildTime { get; private set; }
        public DateTimeOffset? PublicationTime { get; private set; }

        public Workspace(
            string name,
            IWorkspaceInitializer initializer = null,
            bool requiresPublish = false) : this(
            new DirectoryInfo(Path.Combine(DefaultWorkspacesDirectory.FullName, name)),
            name,
            initializer,
            requiresPublish)
        {
        }

        public Workspace(
            DirectoryInfo directory,
            string name = null,
            IWorkspaceInitializer initializer = null,
            bool requiresPublish = false)
        {
            Name = name ?? directory.Name;
            Directory = directory ?? throw new ArgumentNullException(nameof(directory));
            _initializer = initializer ?? new WorkspaceInitializer("console", Name);
            ConstructionTime = Clock.Current.Now();
            RequiresPublish = requiresPublish;
            _created = new AsyncLazy<bool>(VerifyOrCreate);
            _built = new AsyncLazy<bool>(VerifyOrBuild);
            _published = new AsyncLazy<bool>(VerifyOrPublish);
            _log = new Logger($"{nameof(Workspace)}:{Name}");
        }

        private bool IsDirectoryCreated { get; set; }

        public bool IsCreated { get; private set; }

        public bool IsBuilt { get; private set; }

        public bool IsWebProject =>
            _isWebProject ??
            (_isWebProject = Directory.GetDirectories("wwwroot", SearchOption.AllDirectories).Any()).Value;

        public DirectoryInfo Directory { get; }

        public string Name { get; }

        public static DirectoryInfo DefaultWorkspacesDirectory { get; }

        public bool IsPublished { get; private set; }

        public FileInfo EntryPointAssemblyPath
        {
            get
            {
                if (_entryPointAssemblyPath == null)
                {
                    var depsFile = Directory.GetFiles("*.deps.json", SearchOption.AllDirectories).First();

                    var entryPointAssemblyName = DepsFileParser.GetEntryPointAssemblyName(depsFile);

                    var path =
                        Path.Combine(
                            Directory.FullName,
                            "bin",
                            "Debug",
                            TargetFramework);

                    if (IsWebProject)
                    {
                        path = Path.Combine(path, "publish");
                    }

                    _entryPointAssemblyPath = new FileInfo(Path.Combine(path, entryPointAssemblyName));
                }

                return _entryPointAssemblyPath;
            }
        }

        public string TargetFramework => _targetFramework ??
                                         (_targetFramework = RuntimeConfig.GetTargetFramework(
                                              Directory.GetFiles("*.runtimeconfig.json", SearchOption.AllDirectories).First()));

        public DateTimeOffset? ReadyTime { get; set; }

        public async Task EnsureCreated(Budget budget = null)
        {
            await _created
                .ValueAsync()
                .CancelIfExceeds(budget ?? new Budget());
            budget?.RecordEntry();
        }
     
        private async Task<bool> VerifyOrCreate()
        {
            using (var operation = _log.OnEnterAndConfirmOnExit())
            {
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

                if (!IsCreated)
                {
                    if (Directory.GetFiles().Length == 0)
                    {
                        operation.Info("Initializing workspace using {_initializer} in {directory}", _initializer, Directory);
                        await _initializer.Initialize(Directory);
                    }

                    IsCreated = true;
                    CreationTime = Clock.Current.Now();
                }

                operation.Succeed();
            }

            return true;
        }

        public async Task EnsureReady(Budget budget)
        {
            await EnsureCreated(budget);

            await EnsureBuilt(budget);

            if (RequiresPublish)
            {
                await EnsurePublished(budget);
            }
        }

        public bool RequiresPublish { get; }

        public async Task EnsureBuilt(Budget budget = null)
        {
            await EnsureCreated(budget);

            await _built.ValueAsync()
                        .CancelIfExceeds(budget ?? new Budget());
            budget?.RecordEntry();
        }

        private async Task<bool> VerifyOrBuild()
        {
            using (var operation = _log.OnEnterAndConfirmOnExit())
            {
                if (!IsBuilt)
                {
                    operation.Info("Building workspace");
                    if (Directory.GetFiles("*.deps.json", SearchOption.AllDirectories).Length == 0)
                    {
                        operation.Info("Building workspace using {_initializer} in {directory}", _initializer, Directory);
                        var result = await new Dotnet(Directory)
                                         .Build(args: "/fl /p:ProvideCommandLineArgs=true");
                        result.ThrowOnFailure();
                    }

                    IsBuilt = true;
                    BuildTime = Clock.Current.Now();
                }
                else
                {
                    operation.Info("Workspace already built");
                }

                operation.Succeed();
                operation.Info("Workspace built");
            }

            return true;
        }

        public async Task EnsurePublished(Budget budget = null)
        {
            await EnsureBuilt(budget);

            await _published.ValueAsync()
                            .CancelIfExceeds(budget ?? new Budget());
        }

        private async Task<bool> VerifyOrPublish()
        {
            using (var operation = _log.OnEnterAndConfirmOnExit())
            {
                if (!IsPublished)
                {
                    operation.Info("Publishing workspace");
                    if (Directory.GetDirectories("publish", SearchOption.AllDirectories).Length == 0)
                    {
                        operation.Info("Publishing workspace in {directory}", Directory);
                        var result = await new Dotnet(Directory)
                            .Publish("--no-dependencies --no-restore");
                        result.ThrowOnFailure();
                    }

                    IsPublished = true;
                    PublicationTime = Clock.Current.Now();
                    operation.Info("Workspace published");
                }
                else
                {
                    operation.Info("Workspace already published");
                }

                operation.Succeed();
                
            }

            return true;
        }

        public static Workspace Copy(
            Workspace fromWorkspace,
            string folderNameStartsWith = null)
        {
            if (fromWorkspace == null)
            {
                throw new ArgumentNullException(nameof(fromWorkspace));
            }

            folderNameStartsWith = folderNameStartsWith ?? fromWorkspace.Name;
            var parentDirectory = fromWorkspace.Directory.Parent;

            var destination = CreateDirectory(folderNameStartsWith, parentDirectory);

            return Copy(fromWorkspace, destination);
        }

        public static Workspace Copy(Workspace fromWorkspace, DirectoryInfo destination)
        {
            fromWorkspace.Directory.CopyTo(destination);

            var copy = new Workspace(destination, destination.Name)
            {
                IsCreated = fromWorkspace.IsCreated,
                IsPublished = fromWorkspace.IsPublished,
                IsBuilt = fromWorkspace.IsBuilt,
                IsDirectoryCreated = true
            };

            return copy;
        }

        public static DirectoryInfo CreateDirectory(
            string folderNameStartsWith,
            DirectoryInfo parentDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(folderNameStartsWith))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(folderNameStartsWith));
            }

            parentDirectory = parentDirectory ?? DefaultWorkspacesDirectory;

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
