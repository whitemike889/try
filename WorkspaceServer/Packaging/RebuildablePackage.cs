using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

namespace WorkspaceServer.Packaging
{
    public class RebuildablePackage : Package
    {
        public RebuildablePackage(string name = null, IPackageInitializer initializer = null, DirectoryInfo directory = null, IScheduler buildThrottleScheduler = null) 
            : base(name, initializer, directory, buildThrottleScheduler)
        {

        }

        protected override bool ShouldBuild()
        {
            var shouldBuild = base.ShouldBuild();

            if (!shouldBuild && AnalyzerResult != null)
            {
                var newAnalyzerResult = CreateAnalyzerResult();
                if (LastSuccessfulBuildTime != null && new FileInfo(newAnalyzerResult.ProjectFilePath).LastWriteTimeUtc > LastSuccessfulBuildTime)
                {
                    return true;
                }
                if (!newAnalyzerResult.SourceFiles.SequenceEqual(AnalyzerResult.SourceFiles))
                {
                    return true;
                }
                if (newAnalyzerResult.SourceFiles.Any(f => new FileInfo(f).LastWriteTimeUtc > LastSuccessfulBuildTime))
                {
                    return true;
                }
            }

            return shouldBuild;
        }
    }
}
