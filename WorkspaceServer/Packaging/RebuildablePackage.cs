using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

namespace WorkspaceServer.Packaging
{
    public class RebuildablePackage : Package
    {
        public RebuildablePackage(string name = null, IPackageInitializer initializer = null, DirectoryInfo directory = null) : base(name, initializer, directory)
        {

        }

        public async override Task<CSharpCommandLineArguments> GetCommandLineArguments()
        {
            await CleanAndBuild();
            return await base.GetCommandLineArguments();
        }

        private async Task CleanAndBuild()
        {
            await new Dotnet(Directory).Execute("clean");

            Directory.GetFiles("msbuild.log").SingleOrDefault()?.Delete();
            Directory.GetFiles(".trydotnet").SingleOrDefault()?.Delete();
            await Build();
        }
    }
}
