using System;
using System.IO;
using System.Threading;
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

            Log.Info("Workspaces path is {DefaultWorkspacesDirectory}", DefaultWorkspacesDirectory);
        }

        private readonly IWorkspaceInitializer _initializer;

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
        }

        private bool IsDirectoryCreated { get; set; }

        public bool IsCreated { get; private set; }

        public bool IsBuilt { get; private set; }

        public DirectoryInfo Directory { get; }

        public string Name { get; }

        public static DirectoryInfo DefaultWorkspacesDirectory { get; }

        public async Task EnsureCreated(CancellationToken? cancellationToken = null)
        {
            cancellationToken = cancellationToken ?? Clock.Current.CreateCancellationToken(TimeSpan.FromSeconds(45));

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
                    await _initializer.Initialize(Directory, cancellationToken);
                }

                IsCreated = true;
            }
        }

        public void EnsureBuilt(CancellationToken? cancellationToken = null)
        {
            if (!IsBuilt)
            {
                if (Directory.GetFiles("*.deps.json", SearchOption.AllDirectories).Length == 0)
                {
                    Log.Info("Building workspace using {_initializer} in {directory}", _initializer, Directory);
                    new Dotnet(Directory).Build(cancellationToken).ThrowOnFailure();
                }

                IsBuilt = true;
            }
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

            DirectoryInfo destination;
            var i = 0;

            do
            {
                destination = new DirectoryInfo(
                    Path.Combine(
                        fromWorkspace
                            .Directory
                            .Parent
                            .FullName,
                        $"{folderName}.{++i}"));
            } while (destination.Exists);

            fromWorkspace.Directory.CopyTo(destination);

            var copy = new Workspace(destination,
                                     folderName ?? fromWorkspace.Name,
                                     fromWorkspace._initializer);

            copy.IsCreated = fromWorkspace.IsCreated;
            copy.IsBuilt = fromWorkspace.IsBuilt;
            copy.IsDirectoryCreated = true;

            return copy;
        }
    }
}
