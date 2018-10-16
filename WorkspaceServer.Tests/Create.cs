using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MLS.Protocol;
using MLS.Protocol.Execution;
using Recipes;
using WorkspaceServer.Workspaces;

namespace WorkspaceServer.Tests
{
    public static class Create
    {
        public static async Task<WorkspaceBuild> ConsoleWorkspaceCopy([CallerMemberName] string testName = null) =>
            await WorkspaceBuild.Copy(
                await Default.ConsoleWorkspace,
                testName);

        public static async Task<WorkspaceBuild> WebApiWorkspaceCopy([CallerMemberName] string testName = null) =>
            await WorkspaceBuild.Copy(
                await Default.WebApiWorkspace,
                testName);

        public static async Task<WorkspaceBuild> XunitWorkspaceCopy([CallerMemberName] string testName = null) =>
            await WorkspaceBuild.Copy(
                await Default.XunitWorkspace,
                testName);

        public static async Task<WorkspaceBuild> NetstandardWorkspaceCopy([CallerMemberName] string testName = null) =>
            await WorkspaceBuild.Copy(
                await Default.NetstandardWorkspace,
                testName);

        public static WorkspaceBuild EmptyWorkspace([CallerMemberName] string testName = null, IWorkspaceInitializer initializer = null) =>
            new WorkspaceBuild(WorkspaceBuild.CreateDirectory(testName), outputConfiguration: null, initializer: initializer);

        public static string SimpleWorkspaceRequestAsJson(
            string consoleOutput = "Hello!",
            string workspaceType = null)
        {
            var workspace = Workspace.FromSource(
                SimpleConsoleAppCodeWithoutNamespaces(consoleOutput),
                workspaceType,
                "Program.cs"
            );

            return new WorkspaceRequest(workspace).ToJson();
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
