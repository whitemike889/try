using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using MLS.Agent.Tools;
using Pocket;
using Recipes;
using WorkspaceServer.Servers.Roslyn;
using static Pocket.Logger<WorkspaceServer.Workspaces.WorkspaceBuild>;

namespace WorkspaceServer.Workspaces
{
    public class WorkspaceBuild
    {
        const string csharpLanguageVersion = "7.3";

        static WorkspaceBuild()
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

            Log.Info("Workspaces path is {DefaultWorkspacesDirectory}", DefaultWorkspacesDirectory);
        }

        private readonly IWorkspaceInitializer _initializer;
        private static readonly object _lockObj = new object();
        private readonly AsyncLazy<bool> _created;
        private readonly AsyncLazy<bool> _built;
        private readonly AsyncLazy<bool> _published;
        private readonly AsyncLazy<CSharpCommandLineArguments> _csharpCommandLineArguments;
        private bool? _isWebProject;
        private bool? _isUnitTestProject;
        private FileInfo _entryPointAssemblyPath;
        private static string _targetFramework;
        private readonly Logger _log;
        private WorkspaceConfiguration _configuration;
        private readonly AsyncLazy<SyntaxTree> _instrumentationEmitterSyntaxTree;

        public DateTimeOffset? ConstructionTime { get; }
        public DateTimeOffset? CreationTime { get; private set; }
        public DateTimeOffset? BuildTime { get; private set; }
        public DateTimeOffset? PublicationTime { get; private set; }

        public WorkspaceBuild(
            string name,
            IBuildArtifactLocator buildArtifactLocator,
            IWorkspaceInitializer initializer = null,
            bool requiresPublish = false) : this(
            new DirectoryInfo(Path.Combine(DefaultWorkspacesDirectory.FullName, name)),
            buildArtifactLocator,
            name,
            initializer,
            requiresPublish)
        {
        }

        public WorkspaceBuild(
            DirectoryInfo directory,
            IBuildArtifactLocator buildArifactLocator,
            string name = null,
            IWorkspaceInitializer initializer = null,
            bool requiresPublish = false)
        {
            Name = name ?? directory.Name;
            Directory = directory ?? throw new ArgumentNullException(nameof(directory));
            _initializer = initializer ?? new WorkspaceInitializer("console", Name);
            ConstructionTime = Clock.Current.Now();
            RequiresPublish = requiresPublish;
            _csharpCommandLineArguments = new AsyncLazy<CSharpCommandLineArguments>(CreateCSharpCommandLineArguments);
            _instrumentationEmitterSyntaxTree = new AsyncLazy<SyntaxTree>(CreateInstrumentationEmitterSyntaxTree);
            _created = new AsyncLazy<bool>(VerifyOrCreate);
            _built = new AsyncLazy<bool>(VerifyOrBuild);
            _published = new AsyncLazy<bool>(VerifyOrPublish);
            _log = new Logger($"{nameof(WorkspaceBuild)}:{Name}");

            if (buildArifactLocator != null)
            {
                BuildArifactLocator = buildArifactLocator;
            }
            else
            {
                try
                {
                    if (NetCoreAppBuildArtifactLocator.Instance.GetTargetFramework(Directory) != null)
                    {
                        BuildArifactLocator = NetCoreAppBuildArtifactLocator.Instance;
                    }
                }
                catch
                {
                    throw new InvalidOperationException("");
                }
            }
        }

        private bool IsDirectoryCreated { get; set; }

        public bool IsCreated { get; private set; }

        public bool IsBuilt { get; private set; }

        private bool IsReady { get; set; }

        public bool IsUnitTestProject =>
            _isUnitTestProject ??
            (_isUnitTestProject = Directory.GetFiles("*.testadapter.dll", SearchOption.AllDirectories).Any()).Value;

        public bool IsWebProject =>
            _isWebProject ??
            (_isWebProject = Directory.GetDirectories("wwwroot", SearchOption.AllDirectories).Any()).Value;

        public DirectoryInfo Directory { get; }
        public IBuildArtifactLocator BuildArifactLocator { get; }
        public string Name { get; }

        public static DirectoryInfo DefaultWorkspacesDirectory { get; }

        public async Task<WorkspaceConfiguration> GetConfigurationAsync()
        {
            if (_configuration == null)
            {
                await EnsureBuilt();

                var workspaceConfigFile = new FileInfo(Path.Combine(Directory.FullName, ".trydotnet"));

                if (workspaceConfigFile.Exists)
                {
                    var json = await workspaceConfigFile.ReadAsync();

                    _configuration = json.FromJsonTo<WorkspaceConfiguration>();
                }
                else
                {
                    var buildLog = Directory.GetFiles("msbuild.log").SingleOrDefault();

                    if (buildLog == null)
                    {
                        throw new InvalidOperationException($"msbuild.log not found in {Directory}");
                    }

                    var compilerCommandLine = buildLog.FindCompilerCommandLineAndSetLanguageversion(csharpLanguageVersion);

                    _configuration = new WorkspaceConfiguration
                    {
                        CompilerArgs = compilerCommandLine
                    };

                    File.WriteAllText(workspaceConfigFile.FullName,
                                      _configuration.ToJson());
                }
            }

            return _configuration;
        }

        public bool IsPublished { get; private set; }

        public FileInfo EntryPointAssemblyPath
        {
            get
            {
                if (_entryPointAssemblyPath == null)
                {
                    _entryPointAssemblyPath = BuildArifactLocator.GetEntryPointAssemblyPath(Directory, IsWebProject);
                }

                return _entryPointAssemblyPath;
            }
        }

        public string TargetFramework
        {
            get
            {
                if (_targetFramework == null)
                {
                    _targetFramework = BuildArifactLocator.GetTargetFramework(Directory);
                }

                return _targetFramework;
            }
        }

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
            if (IsReady)
            {
                return;
            }

            await EnsureCreated(budget);

            await EnsureBuilt(budget);

            if (RequiresPublish)
            {
                await EnsurePublished(budget);
            }

            IsReady = true;
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
                    var lockFile = new FileInfo(Path.Combine(Directory.FullName, ".trydotnet-lock"));
                    FileStream fileStream = null;

                    try
                    {
                        fileStream = File.Create(lockFile.FullName, 1, FileOptions.DeleteOnClose);

                        operation.Info("Building workspace");
                        if (Directory.GetFiles("*.deps.json", SearchOption.AllDirectories).Length == 0)
                        {
                            operation.Info("Building workspace using {_initializer} in {directory}", _initializer, Directory);
                            var result = await new Dotnet(Directory)
                                             .Build(args: "/fl /p:ProvideCommandLineArgs=true;append=true");
                            result.ThrowOnFailure();
                        }

                        IsBuilt = true;
                        BuildTime = Clock.Current.Now();

                        operation.Info("Workspace built");
                    }
                    catch (Exception exception)
                    {
                        operation.Error("Exception building workspace", exception);
                    }
                    finally
                    {
                        fileStream?.Dispose();
                    }
                }
                else
                {
                    operation.Info("Workspace already built");
                }

                operation.Succeed();
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

        public static async Task<WorkspaceBuild> Copy(
            WorkspaceBuild fromWorkspaceBuild,
            string folderNameStartsWith = null)
        {
            if (fromWorkspaceBuild == null)
            {
                throw new ArgumentNullException(nameof(fromWorkspaceBuild));
            }

            await fromWorkspaceBuild.EnsureReady(new Budget());

            folderNameStartsWith = folderNameStartsWith ?? fromWorkspaceBuild.Name;
            var parentDirectory = fromWorkspaceBuild.Directory.Parent;

            var destination = CreateDirectory(folderNameStartsWith, parentDirectory);

            return await Copy(fromWorkspaceBuild, destination);
        }

        public static async Task<WorkspaceBuild> Copy(
            WorkspaceBuild fromWorkspaceBuild,
            DirectoryInfo destination)
        {
            if (fromWorkspaceBuild == null)
            {
                throw new ArgumentNullException(nameof(fromWorkspaceBuild));
            }

            await fromWorkspaceBuild.EnsureReady(new Budget());

            fromWorkspaceBuild.Directory.CopyTo(destination);

            var copy = new WorkspaceBuild(destination, fromWorkspaceBuild.BuildArifactLocator, destination.Name)
            {
                IsCreated = fromWorkspaceBuild.IsCreated,
                IsPublished = fromWorkspaceBuild.IsPublished,
                IsBuilt = fromWorkspaceBuild.IsBuilt,
                IsReady = fromWorkspaceBuild.IsReady,
                IsDirectoryCreated = true,

            };

            Log.Info(
                "Copied workspace {from} to {to}",
                fromWorkspaceBuild,
                copy);

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

        public override string ToString()
        {
            return $"{Name} ({Directory.FullName}) ({new { IsCreated, CreationTime, IsBuilt, BuildTime, IsPublished, PublicationTime, IsReady }})";
        }

        public async Task<AdhocWorkspace> CreateRoslynWorkspace(ProjectId projectId = null)
        {
            projectId = projectId ?? ProjectId.CreateNewId(Name);
            CSharpCommandLineArguments csharpCommandLineArguments = await GetCommandLineArguments();

            var projectInfo = CommandLineProject.CreateProjectInfo(
                projectId,
                Name,
                csharpCommandLineArguments.CompilationOptions.Language,
                csharpCommandLineArguments,
                Directory.FullName);

            var workspace = new AdhocWorkspace(MefHostServices.DefaultHost);

            workspace.AddProject(projectInfo);

            return workspace;
        }

        private async Task<CSharpCommandLineArguments> CreateCSharpCommandLineArguments()
        {
            await EnsureBuilt();
            return CSharpCommandLineParser.Default.Parse(
                (await GetConfigurationAsync()).CompilerArgs,
                Directory.FullName,
                RuntimeEnvironment.GetRuntimeDirectory());
        }

        private async Task<SyntaxTree> CreateInstrumentationEmitterSyntaxTree()
        {
            var resourceName = "WorkspaceServer.Servers.Roslyn.Instrumentation.InstrumentationEmitter.cs";

            var assembly = typeof(WorkspaceBuildExtensions).Assembly;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream ?? throw new InvalidOperationException($"Resource \"{resourceName}\" not found"), Encoding.UTF8))
            {
                var source = reader.ReadToEnd();

                var parseOptions = (await _csharpCommandLineArguments.ValueAsync()).ParseOptions;
                var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source), parseOptions);

                return syntaxTree;
            }
        }

        public Task<CSharpCommandLineArguments> GetCommandLineArguments() => _csharpCommandLineArguments.ValueAsync();

        public Task<SyntaxTree> GetInstrumentationEmitterSyntaxTree() => _instrumentationEmitterSyntaxTree.ValueAsync();
    }
}
