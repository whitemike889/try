using System.IO;
using System.Reactive.Concurrency;

namespace WorkspaceServer.Packaging
{
    public class NonrebuildablePackage : Package
    {

        public NonrebuildablePackage(string name = null, IPackageInitializer initializer = null, DirectoryInfo directory = null, IScheduler buildThrottleScheduler = null) 
            : base(name, initializer, directory, buildThrottleScheduler)
        {

        }
        
    }
}
