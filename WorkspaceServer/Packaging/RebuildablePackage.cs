using System.IO;
using System.Linq;
using System.Reactive.Concurrency;

namespace WorkspaceServer.Packaging
{
    public class RebuildablePackage : Package
    {
        private FileSystemWatcher _fileSystemWatcher;

        public RebuildablePackage(string name = null, IPackageInitializer initializer = null, DirectoryInfo directory = null, IScheduler buildThrottleScheduler = null) 
            : base(name, initializer, directory, buildThrottleScheduler)
        {

            _fileSystemWatcher = new FileSystemWatcher(Directory.FullName)
            {
                EnableRaisingEvents = true
            };

            _fileSystemWatcher.Changed += FileSystemWatcherOnChangedOrDeleted;
            _fileSystemWatcher.Deleted += FileSystemWatcherOnDeleted;
            _fileSystemWatcher.Renamed += FileSystemWatcherOnRenamed;
            _fileSystemWatcher.Created += FileSystemWatcherOnCreated;
        }

        private void FileSystemWatcherOnDeleted(object sender, FileSystemEventArgs e)
        {
            HandleFileChanges(e.Name);
        }

        private void FileSystemWatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (e.Name.EndsWith(".csproj") || e.Name.EndsWith(".cs"))
            {
                DesignTimeBuildResult = null;
            }
        }

        private void FileSystemWatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            HandleFileChanges(e.OldName);
        }

        private void HandleFileChanges(string fileName)
        {
            if (DesignTimeBuildResult != null)
            {
                if (fileName.EndsWith(".csproj"))
                {
                    DesignTimeBuildResult = null;
                }
                else if (fileName.EndsWith(".cs"))
                {
                    var analyzerInputs = DesignTimeBuildResult.GetCompileInputs();
                    if (analyzerInputs.Any(sourceFile => sourceFile.EndsWith(fileName)))
                    {
                        DesignTimeBuildResult = null;
                    }
                }
            }
        }

        private void FileSystemWatcherOnChangedOrDeleted(object sender, FileSystemEventArgs e)
        {
            HandleFileChanges(e.Name);
        }
    }
}
