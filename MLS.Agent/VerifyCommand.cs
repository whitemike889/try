using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MLS.Agent.Markdown;
using MLS.Protocol;
using MLS.Protocol.Execution;
using WorkspaceServer;
using WorkspaceServer.Servers.Roslyn;

namespace MLS.Agent
{
    public static class VerifyCommand
    {
        public static async Task<int> Do(
            DirectoryInfo rootDirectory,
            IConsole console,
            Func<IDirectoryAccessor> getDirectoryAccessor,
            PackageRegistry packageRegistry,
            bool compile)
        {
            var directoryAccessor = getDirectoryAccessor();
            var markdownProject = new MarkdownProject(directoryAccessor, packageRegistry);
            var returnCode = 0;
            var workspaceServer =new Lazy<RoslynWorkspaceServer>(() => new RoslynWorkspaceServer(packageRegistry));
            
            foreach (var markdownFile in markdownProject.GetAllMarkdownFiles())
            {
                var fullName = directoryAccessor.GetFullyQualifiedPath(markdownFile.Path).FullName;

                console.Out.WriteLine();
                console.Out.WriteLine(fullName);
                console.Out.WriteLine(new string('-', fullName.Length));

                var codeLinkBlocks = await markdownFile.GetCodeLinkBlocks();
                var sessions = codeLinkBlocks.GroupBy(block => block.Session);

                foreach (var session in sessions)
                {
                    if (session.Select(s => s.ProjectOrPackageName()).Distinct().Count() != 1)
                    {
                        SetError();
                        console.Out.WriteLine($"Session cannot span projects or packages: --session {session.Key}");
                        continue;
                    }

                    foreach (var codeLinkBlock in session)
                    {
                        VerifyCodeLinkage(codeLinkBlock);
                    }

                    Console.ResetColor();

                    if (compile)
                    {
                        await VerifyCompilation(session, markdownFile);
                    }

                    Console.ResetColor();
                }
            }

            return returnCode;

            void SetError()
            {
                Console.ForegroundColor = ConsoleColor.Red;
                returnCode = 1;
            }

            async Task VerifyCompilation(IGrouping<string, CodeLinkBlock> session, MarkdownFile markdownFile)
            {
                console.Out.WriteLine($"\n  Compiling samples for session \"{session.Key}\"\n");

                var projectOrPackageName = session.First().ProjectOrPackageName();

                var buffers = session.Select(block =>
                {
                    var absolutePath = directoryAccessor
                                       .GetDirectoryAccessorForRelativePath(markdownFile.Path.Directory)
                                       .GetFullyQualifiedPath(block.SourceFile).FullName;
                    var bufferId = new BufferId(absolutePath, block.Region);
                    return new Workspace.Buffer(bufferId, block.SourceCode);
                }).ToArray();

                var workspace = new Workspace(
                    workspaceType: projectOrPackageName,
                    buffers: buffers);

                var result = await workspaceServer.Value.Compile(new WorkspaceRequest(workspace));

                var symbol = !result.Succeeded
                                 ? "X"
                                 : "✓";

                if (result.Succeeded)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    console.Out.WriteLine($"    {symbol}  Compiles for session {session.Key}");
                }
                else
                {
                    SetError();

                    console.Out.WriteLine($"    {symbol}  Compile failed for session {session.Key}");

                    foreach (var diagnostic in result.GetFeature<Diagnostics>())
                    {
                        console.Out.WriteLine($"\t\t{diagnostic.Message}");
                    }
                }
            }

            void VerifyCodeLinkage(CodeLinkBlock codeLinkBlock)
            {
                var diagnostics = codeLinkBlock.Diagnostics.ToArray();

                if (diagnostics.Any())
                {
                    SetError();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                var sourceFile =
                    codeLinkBlock.SourceFile != null
                        ? directoryAccessor.GetFullyQualifiedPath(codeLinkBlock.SourceFile).FullName
                        : "UNKNOWN";

                var project = codeLinkBlock.ProjectFile?.FullName ?? "UNKNOWN";

                var symbol = diagnostics.Any()
                                 ? "X"
                                 : "✓";

                console.Out.WriteLine($"    {symbol}  Line {codeLinkBlock.Line + 1}:\t{sourceFile} (in project {project})");

                foreach (var diagnostic in diagnostics)
                {
                    console.Out.WriteLine($"\t\t{diagnostic}");
                }
            }
        }
    }

}