using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Clockwise;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MLS.Agent.Tools;
using System.Reactive.Linq;
using Pocket;
using WorkspaceServer.Servers.Roslyn;
using static Pocket.Logger<WorkspaceServer.Packaging.Package>;

namespace WorkspaceServer.Packaging
{
    public abstract class Package
    {
        const string csharpLanguageVersion = "7.3";

        static Package()
        {
            const string workspacesPathEnvironmentVariableName = "TRYDOTNET_PACKAGES_PATH";

            var environmentVariable = Environment.GetEnvironmentVariable(workspacesPathEnvironmentVariableName);

            DefaultPackagesDirectory =
                environmentVariable != null
                    ? new DirectoryInfo(environmentVariable)
                    : new DirectoryInfo(
                        Path.Combine(
                            Paths.UserProfile,
                            ".trydotnet",
                            "packages"));

            if (!DefaultPackagesDirectory.Exists)
            {
                DefaultPackagesDirectory.Create();
            }

            Log.Info("Packages path is {DefaultWorkspacesDirectory}", DefaultPackagesDirectory);
        }

        private readonly IPackageInitializer _initializer;
        private static readonly object _lockObj = new object();
        private bool? _isWebProject;
        private bool? _isUnitTestProject;
        private FileInfo _entryPointAssemblyPath;
        private static string _targetFramework;
        private string _lastDesignTimeBuildError;
        private readonly Logger _log;
        private readonly Subject<Budget> _buildRequestChannel;
        private TaskCompletionSource<Workspace> _workspaceCompletionSource;
        private readonly IScheduler _buildThrottleScheduler;
        private SerialDisposable _buildThrottlerSubscription;

        protected Package(
            string name = null,
            IPackageInitializer initializer = null,
            DirectoryInfo directory = null,
            IScheduler buildThrottleScheduler = null)
        {
            Name = name ?? directory?.Name ?? throw new ArgumentException($"You must specify {nameof(name)}, {nameof(directory)}, or both.");
            _initializer = initializer ?? new PackageInitializer("console", Name);
            ConstructionTime = Clock.Current.Now();
            Directory = directory ?? new DirectoryInfo(Path.Combine(DefaultPackagesDirectory.FullName, Name));
            LastBuildErrorLogFile = new FileInfo(Path.Combine(Directory.FullName, ".trydotnet-builderror"));
            _log = new Logger($"{nameof(Package)}:{Name}");
            _buildThrottleScheduler = buildThrottleScheduler ?? TaskPoolScheduler.Default;
            _buildRequestChannel = new Subject<Budget>();
            _buildThrottlerSubscription = new SerialDisposable();
            SetupWorkspaceCreationChannel();
            TryDiscoverPreviousBuild();
        }


        private void TryDiscoverPreviousBuild()
        {
            if (Directory.Exists)
            {
                var binLog = Directory.GetFiles("*.binlog").FirstOrDefault();
                if (binLog != null)
                {
                    var projectFile = Directory.GetFiles("*.csproj").FirstOrDefault();
                    if (projectFile != null && binLog.LastWriteTimeUtc >= projectFile.LastWriteTimeUtc)
                    {
                        var manager = new AnalyzerManager();
                        var result = manager.Analyze(binLog.FullName).FirstOrDefault(p => p.ProjectFilePath == projectFile.FullName);
                        if (result != null && result.Succeeded)
                        {
                            AnalyzerResult = result;
                            LastSuccessfulBuildTime = binLog.LastWriteTimeUtc;
                        }
                    }
                }
            }
        }

        private bool IsDirectoryCreated { get; set; }
        private FileInfo LastBuildErrorLogFile { get; }

        public DateTimeOffset? ConstructionTime { get; }

        public DateTimeOffset? CreationTime { get; private set; }

        public DateTimeOffset? LastSuccessfulBuildTime { get; private set; }

        public DateTimeOffset? PublicationTime { get; private set; }

        public bool IsUnitTestProject =>
            _isUnitTestProject ??
            (_isUnitTestProject = Directory.GetFiles("*.testadapter.dll", SearchOption.AllDirectories).Any()).Value;

        public bool IsWebProject
        {
            get
            {
                if (_isWebProject == null &&
                    Directory.GetFiles("*.csproj").SingleOrDefault() is FileInfo csproj)
                {
                    var csprojXml = File.ReadAllText(csproj.FullName);

                    var xml = XElement.Parse(csprojXml);

                    var wut = xml.XPathSelectElement("//ItemGroup/PackageReference[@Include='Microsoft.AspNetCore.App']");

                    _isWebProject = wut != null;
                }

                return _isWebProject ?? false;
            }
        }

        public DirectoryInfo Directory { get; }

        public string Name { get; }

        public static DirectoryInfo DefaultPackagesDirectory { get; }

        public FileInfo EntryPointAssemblyPath => _entryPointAssemblyPath ?? (_entryPointAssemblyPath = GetEntryPointAssemblyPath(Directory, IsWebProject));

        public string TargetFramework => _targetFramework ?? (_targetFramework = GetTargetFramework(Directory));

        public DateTimeOffset? ReadyTime { get; set; }

        protected async Task<bool> Create()
        {
            using (var operation = _log.OnEnterAndConfirmOnExit())
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

                if (Directory.GetFiles().Length == 0)
                {
                    operation.Info("Initializing package using {_initializer} in {directory}", _initializer, Directory);
                    await _initializer.Initialize(Directory);
                }

                CreationTime = Clock.Current.Now();
                operation.Succeed();
                return true;
            }
        }

        public Task<Workspace> CreateRoslynWorkspaceAsync(Budget budget)
        {
            if (!ShouldBuild())
            {
                var ws = CreateRoslynWorkspace();
                return Task.FromResult(ws);
            }

            if (_workspaceCompletionSource == null)
            {
                _workspaceCompletionSource = new TaskCompletionSource<Workspace>();
            }

            _buildRequestChannel.OnNext(budget);
            return _workspaceCompletionSource.Task;
        }

        private void SetupWorkspaceCreationChannel()
        {

            _buildThrottlerSubscription.Disposable = _buildRequestChannel
                .Throttle(TimeSpan.FromSeconds(0.5), _buildThrottleScheduler)
                .Subscribe(
                      async (budget) =>
                      {
                          await ProcessBuildRequest(budget);
                      },
                  error =>
                  {
                      _workspaceCompletionSource?.SetException(error);
                      _workspaceCompletionSource = null;

                      SetupWorkspaceCreationChannel();
                  });
        }

        private async Task ProcessBuildRequest(Budget budget)
        {
            var ws = await InternalCreateRoslynWorkspaceAsync(budget);
            _workspaceCompletionSource?.SetResult(ws);
            _workspaceCompletionSource = null;
        }

        private async Task<Workspace> InternalCreateRoslynWorkspaceAsync(Budget budget)
        {
            await EnsureReady(budget);
            return CreateRoslynWorkspace();
        }

        private Workspace CreateRoslynWorkspace()
        {
            var ws = AnalyzerResult.GetWorkspace();
            var projectId = ws.CurrentSolution.ProjectIds.FirstOrDefault();
            ws.TryApplyChanges(ws.CurrentSolution.WithProjectMetadataReferences(projectId,
                AnalyzerResult.References.GetMetadataReferences()));
            return ws;
        }

        public virtual async Task EnsureReady(Budget budget)
        {
            budget = budget ?? new Budget();

            await EnsureCreated().CancelIfExceeds(budget);

            await EnsureBuilt().CancelIfExceeds(budget);

            if (RequiresPublish)
            {
                await EnsurePublished().CancelIfExceeds(budget);
            }

            budget.RecordEntry();
        }

        protected virtual async Task<bool> EnsureCreated() => await Create();

        protected virtual async Task<bool> EnsureBuilt() => await EnsureCreated() && await Build();

        public virtual async Task<bool> EnsurePublished()
        {
            return (await EnsureBuilt()) &&
            ((await Publish()).ExitCode == 0);
        }

        public bool RequiresPublish => IsWebProject;

        protected async Task<bool> Build()
        {
            using (var operation = _log.OnEnterAndConfirmOnExit())
            {
                var lockFile = new FileInfo(Path.Combine(Directory.FullName, ".trydotnet-lock"));
                FileStream fileStream = null;
                try
                {
                    fileStream = File.Create(lockFile.FullName, 1, FileOptions.DeleteOnClose);
                    if (ShouldBuild())
                    {
                        AnalyzerResult = CreateAnalyzerResult();
                        if (AnalyzerResult?.Succeeded == false)
                        {
                            File.WriteAllText(
                                LastBuildErrorLogFile.FullName,
                                _lastDesignTimeBuildError);
                        }
                        else
                        {
                            operation.Info("Building workspace using {_initializer} in {directory}", _initializer, Directory);
                            var result = await new Dotnet(Directory)
                                             .Build(args: "/bl");

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
                            LastSuccessfulBuildTime = Directory.GetFiles("*.binlog").FirstOrDefault().LastWriteTimeUtc;
                            operation.Info("Workspace built");
                        }
                    }
                    else
                    {
                        operation.Info("Workspace already built");
                    }

                    operation.Succeed();
                    return true;
                }
                catch (Exception exception)
                {
                    operation.Error("Exception building workspace", exception);
                    return false;
                }
                finally
                {
                    fileStream?.Dispose();
                }
            }
        }

        protected async Task<CommandLineResult> Publish()
        {
            using (var operation = _log.OnEnterAndConfirmOnExit())
            {
                operation.Info("Publishing workspace in {directory}", Directory);
                var result = await new Dotnet(Directory)
                    .Publish("--no-dependencies --no-restore");
                result.ThrowOnFailure();

                PublicationTime = Clock.Current.Now();
                operation.Info("Workspace published");
                operation.Succeed();
                return result;
            }
        }

        public static async Task<Package> Copy(
            Package fromPackage,
            string folderNameStartsWith = null,
            bool isRebuildable = false,
            IScheduler buildThrottleScheduler = null)
        {
            if (fromPackage == null)
            {
                throw new ArgumentNullException(nameof(fromPackage));
            }

            await fromPackage.EnsureReady(new Budget());

            folderNameStartsWith = folderNameStartsWith ?? fromPackage.Name;
            var parentDirectory = fromPackage.Directory.Parent;

            var destination = CreateDirectory(folderNameStartsWith, parentDirectory);

            return await Copy(fromPackage, destination, isRebuildable, buildThrottleScheduler);
        }

        private static async Task<Package> Copy(Package fromPackage,
            DirectoryInfo destination,
            bool isRebuildable,
            IScheduler buildThrottleScheduler)
        {
            if (fromPackage == null)
            {
                throw new ArgumentNullException(nameof(fromPackage));
            }

            await fromPackage.EnsureReady(new Budget());

            fromPackage.Directory.CopyTo(destination);

            Package copy;
            if (isRebuildable)
            {
                copy = new RebuildablePackage(directory: destination, name: destination.Name, buildThrottleScheduler: buildThrottleScheduler)
                {
                    IsDirectoryCreated = true
                };
            }
            else
            {
                copy = new NonrebuildablePackage(directory: destination, name: destination.Name, buildThrottleScheduler: buildThrottleScheduler)
                {
                    IsDirectoryCreated = true
                };
            }

            Log.Info(
                "Copied workspace {from} to {to}",
                fromPackage,
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

            parentDirectory = parentDirectory ?? DefaultPackagesDirectory;

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
            return $"{Name} ({Directory.FullName}) ({new { CreationTime, LastSuccessfulBuildTime, PublicationTime }})";
        }

        protected SyntaxTree CreateInstrumentationEmitterSyntaxTree()
        {
            var resourceName = "WorkspaceServer.Servers.Roslyn.Instrumentation.InstrumentationEmitter.cs";

            var assembly = typeof(PackageExtensions).Assembly;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream ?? throw new InvalidOperationException($"Resource \"{resourceName}\" not found"), Encoding.UTF8))
            {
                var source = reader.ReadToEnd();

                var parseOptions = AnalyzerResult.GetCSharpParseOptions();
                var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source), parseOptions);

                return syntaxTree;
            }
        }

        public AnalyzerResult AnalyzerResult { get; protected set; }

        protected virtual bool ShouldBuild()
        {
            return AnalyzerResult == null || (AnalyzerResult.Succeeded == false);
        }

        protected AnalyzerResult CreateAnalyzerResult()
        {
            var csProj = Directory.GetFiles("*.csproj").FirstOrDefault();
            var logWriter = new StringWriter();
            var manager = new AnalyzerManager(new AnalyzerManagerOptions
            {
                LogWriter = logWriter
            });

            var analyzer = manager.GetProject(csProj.FullName);
            analyzer.SetGlobalProperty("langVersion", csharpLanguageVersion);
            var result = analyzer.Build().Results.First();
            if (result.Succeeded == false)
            {
                _lastDesignTimeBuildError = logWriter.ToString();
            }
            else
            {
                _lastDesignTimeBuildError = string.Empty;
            }

            return result;
        }

        public virtual SyntaxTree GetInstrumentationEmitterSyntaxTree() =>
            CreateInstrumentationEmitterSyntaxTree();

        private static string GetTargetFramework(DirectoryInfo directory)
        {
            var runtimeConfig = directory.GetFiles("*.runtimeconfig.json", SearchOption.AllDirectories).FirstOrDefault();

            if (runtimeConfig != null)
            {
                return RuntimeConfig.GetTargetFramework(runtimeConfig);
            }
            else
            {
                return "netstandard2.0";
            }
        }

        private static FileInfo GetEntryPointAssemblyPath(DirectoryInfo directory, bool isWebProject)
        {
            var depsFile = directory.GetFiles("*.deps.json", SearchOption.AllDirectories).FirstOrDefault();

            if (depsFile == null)
            {
                return null;
            }

            var entryPointAssemblyName = DepsFileParser.GetEntryPointAssemblyName(depsFile);

            var path =
                Path.Combine(
                    directory.FullName,
                    "bin",
                    "Debug",
                    GetTargetFramework(directory));

            if (isWebProject)
            {
                path = Path.Combine(path, "publish");
            }

            return new FileInfo(Path.Combine(path, entryPointAssemblyName));
        }
    }
}
