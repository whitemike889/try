using System;
using System.IO;
using System.Threading.Tasks;
using Clockwise;
using Pocket;
using static Pocket.Logger<MLS.Agent.Tools.Workspace>;

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

            Log.Info("Workspaces path is {DefaultWorkspacesDirectory}", DefaultWorkspacesDirectory);
        }

        private readonly IWorkspaceInitializer _initializer;

        private readonly AsyncLazy<bool> _created;
        private readonly AsyncLazy<bool> _built;

        public Workspace(
            string name,
            IWorkspaceInitializer initializer = null) : this(
            new DirectoryInfo(Path.Combine(DefaultWorkspacesDirectory.FullName, name)),
            name,
            initializer)
        {
        }

        public Workspace(
            DirectoryInfo directory,
            string name = null,
            IWorkspaceInitializer initializer = null)
        {
            Name = name ?? directory.Name;
            Directory = directory ?? throw new ArgumentNullException(nameof(directory));
            _initializer = initializer ?? new DotnetWorkspaceInitializer("console", Name);

            _created = new AsyncLazy<bool>(VerifyOrCreate);
            _built = new AsyncLazy<bool>(VerifyOrBuild);
        }

        private bool IsDirectoryCreated { get; set; }

        public bool IsCreated { get; private set; }

        public bool IsBuilt { get; private set; }

        public DirectoryInfo Directory { get; }

        public string Name { get; }

        public static DirectoryInfo DefaultWorkspacesDirectory { get; }

        public async Task EnsureCreated(TimeBudget budget = null) =>
            await _created.ValueAsync()
                          .CancelIfExceeds(budget ?? TimeBudget.Unlimited());

        private async Task<bool> VerifyOrCreate()
        {
            if (!IsDirectoryCreated)
            {
                Directory.Refresh();

                if (!Directory.Exists)
                {
                    Log.Info("Creating directory {directory}", Directory);
                    Directory.Create();
                    Directory.Refresh();
                }

                IsDirectoryCreated = true;
            }

            if (!IsCreated)
            {
                if (Directory.GetFiles().Length == 0)
                {
                    Log.Info("Initializing workspace using {_initializer} in {directory}", _initializer, Directory);
                    await _initializer.Initialize(Directory);
                }

                IsCreated = true;
            }

            return true;
        }

        public async Task EnsureBuilt(TimeBudget budget = null)
        {
            await EnsureCreated(budget);
            await _built.ValueAsync()
                        .CancelIfExceeds(budget ?? TimeBudget.Unlimited());
        }

        private async Task<bool> VerifyOrBuild()
        {
            if (!IsBuilt)
            {
                if (Directory.GetFiles("*.deps.json", SearchOption.AllDirectories).Length == 0)
                {
                    Log.Info("Building workspace using {_initializer} in {directory}", _initializer, Directory);
                    var result = await new Dotnet(Directory)
                                     .Build(
                                         args: "--no-dependencies");
                    result.ThrowOnFailure();
                }

                IsBuilt = true;
            }

            return true;
        }

        public static Workspace Copy(
            Workspace fromWorkspace,
            string folderName = null)
        {
            if (fromWorkspace == null)
            {
                throw new ArgumentNullException(nameof(fromWorkspace));
            }

            folderName = folderName ?? fromWorkspace.Name;
            var parentDirectory = fromWorkspace
                                      .Directory
                                      .Parent;

            var destination = CreateDirectory(folderName, parentDirectory);

            fromWorkspace.Directory.CopyTo(destination);

            var copy = new Workspace(destination,
                                     folderName,
                                     fromWorkspace._initializer);

            copy.IsCreated = fromWorkspace.IsCreated;
            copy.IsBuilt = fromWorkspace.IsBuilt;
            copy.IsDirectoryCreated = true;

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

            DirectoryInfo destination;
            var i = 0;

            do
            {
                destination = new DirectoryInfo(
                    Path.Combine(
                        parentDirectory.FullName,
                        $"{folderNameStartsWith}.{++i}"));
            } while (destination.Exists);

            destination.Create();

            return destination;
        }
    }
}
