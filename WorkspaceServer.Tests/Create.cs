using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MLS.Protocol;
using MLS.Protocol.Execution;
using Recipes;
using WorkspaceServer.Packaging;

namespace WorkspaceServer.Tests
{
    public static class Create
    {
        public static async Task<Package> ConsoleWorkspaceCopy([CallerMemberName] string testName = null, bool isRebuildable =false) =>
            await Package.Copy(
                await Default.ConsoleWorkspace,
                testName,
                isRebuildable);

        public static async Task<Package> WebApiWorkspaceCopy([CallerMemberName] string testName = null) =>
            await Package.Copy(
                await Default.WebApiWorkspace,
                testName);

        public static async Task<Package> XunitWorkspaceCopy([CallerMemberName] string testName = null) =>
            await Package.Copy(
                await Default.XunitWorkspace,
                testName);

        public static async Task<Package> NetstandardWorkspaceCopy([CallerMemberName] string testName = null) =>
            await Package.Copy(
                await Default.NetstandardWorkspace,
                testName);

        public static Package EmptyWorkspace([CallerMemberName] string testName = null, IPackageInitializer initializer = null, bool isRebuildablePackage = false)
        {
            if(!isRebuildablePackage)
            {
                return new NonrebuildablePackage(directory: Package.CreateDirectory(testName), initializer: initializer);
            }

            return new RebuildablePackage(directory: Package.CreateDirectory(testName), initializer: initializer);
        }
            

        public static string SimpleWorkspaceRequestAsJson(
            string consoleOutput = "Hello!",
            string workspaceType = null)
        {
            var workspace = Workspace.FromSource(
                SimpleConsoleAppCodeWithoutNamespaces(consoleOutput),
                workspaceType,
                "Program.cs"
            );

            return new WorkspaceRequest(workspace, requestId: "TestRun").ToJson();
        }

        public static string SimpleConsoleAppCodeWithoutNamespaces(string consoleOutput)
        {
            var code = $@"
using System;

public static class Hello
{{
    public static void Main()
    {{
        Console.WriteLine(""{consoleOutput}"");
    }}
}}";
            return CodeManipulation.EnforceLF(code);
        }
    }
}
