using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using MLS.Agent.Tools;

namespace WorkspaceServer.Servers.Roslyn
{
    public static class WorkspaceBuildExtensions
    {
        public static async Task<AdhocWorkspace> GetRoslynWorkspace(this WorkspaceBuild build, ProjectId projectId = null)
        {
            await build.EnsureBuilt();

            ProjectInfo projectInfo;

            projectId = projectId ?? ProjectId.CreateNewId(build.Name);

            var buildLog = build.Directory.GetFiles("msbuild.log").SingleOrDefault();

            string[] commandLineArgs = buildLog?.FindCompilerCommandLine()?.ToArray();

            if (commandLineArgs?.Any() == true)
            {
                var csharpCommandLineArguments = CSharpCommandLineParser.Default.Parse(
                    commandLineArgs,
                    build.Directory.FullName,
                    RuntimeEnvironment.GetRuntimeDirectory());

                projectInfo = CommandLineProject.CreateProjectInfo(
                    projectId,
                    build.Name,
                    csharpCommandLineArguments.CompilationOptions.Language,
                    csharpCommandLineArguments,
                    build.Directory.FullName);
            }
            else
            {
                projectInfo = ProjectInfo.Create(
                    projectId,
                    version: VersionStamp.Create(),
                    name: build.Name,
                    assemblyName: build.Name,
                    language: LanguageNames.CSharp,
                    compilationOptions: new CSharpCompilationOptions(OutputKind.ConsoleApplication),
                    metadataReferences: WorkspaceUtilities.DefaultReferencedAssemblies);
            }

            var workspace = new AdhocWorkspace(MefHostServices.DefaultHost);

            workspace.AddProject(projectInfo);

            return workspace;
        }
    }
}
