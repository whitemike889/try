using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using MLS.TestSupport;
using Recipes;
using Workspace = MLS.Agent.Tools.Workspace;

namespace WorkspaceServer.Tests
{
    public static class Create
    {
        public static async Task<Workspace> ConsoleWorkspaceCopy([CallerMemberName] string testName = null)
        {
            var workspace = Workspace.Copy(
                await Default.ConsoleWorkspace,
                testName);

            await workspace.EnsureBuilt();

            return workspace;
        }

        public static async Task<Workspace> WebApiWorkspaceCopy([CallerMemberName] string testName = null)
        {
            var workspace = Workspace.Copy(
                await Default.WebApiWorkspace,
                testName);

            return workspace;
        }

        public static Workspace EmptyWorkspace([CallerMemberName] string testName = null, IWorkspaceInitializer initializer = null) =>
            new Workspace(Workspace.CreateDirectory(testName), initializer: initializer);

        public static Models.Execution.Workspace SimpleRunRequest(
            string consoleOutput = "Hello!",
            string workspaceType = null) =>
            SimpleWorkspace(consoleOutput, workspaceType);

        public static Models.Execution.Workspace SimpleWorkspace(
            string consoleOutput = "Hello!",
            string workspaceType = null) =>
            new Models.Execution.Workspace(SimpleConsoleAppCodeWithoutNamespaces(consoleOutput), workspaceType: workspaceType);

        public static string SimpleWorkspaceAsJson(
            string consoleOutput = "Hello!",
            string workspaceType = null) =>
            new
            {
                buffer = SimpleConsoleAppCodeWithoutNamespaces(consoleOutput),
                workspaceType
            }.ToJson();

        public static string SimpleConsoleAppCodeWithoutNamespaces(string consoleOutput)
        {
            var code =  $@"
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
