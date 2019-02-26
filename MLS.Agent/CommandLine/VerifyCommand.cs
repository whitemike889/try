using System;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using MLS.Agent.Markdown;
using MLS.Protocol;
using MLS.Protocol.Diagnostics;
using MLS.Protocol.Execution;
using WorkspaceServer;
using WorkspaceServer.Servers.Roslyn;

namespace MLS.Agent.CommandLine
{
    public static class VerifyCommand
    {
        public static async Task<int> Do(
            VerifyOptions options,
            IConsole console,
            Func<IDirectoryAccessor> getDirectoryAccessor,
            PackageRegistry packageRegistry)
        {
            var directoryAccessor = getDirectoryAccessor();
            var markdownProject = new MarkdownProject(directoryAccessor, packageRegistry);
            var returnCode = 0;
            var workspaceServer = new Lazy<RoslynWorkspaceServer>(() => new RoslynWorkspaceServer(packageRegistry));

            var markdownFiles = markdownProject.GetAllMarkdownFiles().ToArray();

            if (markdownFiles.Length == 0)
            {
                return -1;
            }

            foreach (var markdownFile in markdownFiles)
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
                        ReportCodeLinkageResults(codeLinkBlock);
                    }

                    Console.ResetColor();

                    if (!session.Any(block => block.Diagnostics.Any()))
                    {
                        await ReportCompileResults(session, markdownFile);
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

            async Task ReportCompileResults(IGrouping<string, CodeLinkBlock> session, MarkdownFile markdownFile)
            {
                console.Out.WriteLine($"\n  Compiling samples for session \"{session.Key}\"\n");

                var projectOrPackageName = session.First().ProjectOrPackageName();

                var buffers = await Task.WhenAll(session.Select(block => block.GetBufferAsync(directoryAccessor, markdownFile)).ToArray());

                var workspace = new Workspace(
                    workspaceType: projectOrPackageName,
                    buffers: buffers);

                var result = await workspaceServer.Value.Compile(new WorkspaceRequest(workspace));

                var projectDiagnostics = result.GetFeature<ProjectDiagnostics>()
                                               .Where(e => e.Severity == DiagnosticSeverity.Error)
                                               .ToArray();
                if (projectDiagnostics.Any())
                {
                    SetError();

                    console.Out.WriteLine($"    Build failed for project {session.First().ProjectOrPackageName()}");

                    foreach (var diagnostic in projectDiagnostics)
                    {
                        console.Out.WriteLine($"\t\t{diagnostic.Location}: {diagnostic.Message}");
                    }
                }
                else
                {
                    var symbol = !result.Succeeded
                                     ? "X"
                                     : "✓";

                    if (result.Succeeded)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        console.Out.WriteLine($"    {symbol}  No errors found within samples for session \"{session.Key}\"");
                    }
                    else
                    {
                        SetError();

                        console.Out.WriteLine($"    {symbol}  Errors found within samples for session \"{session.Key}\"");

                        foreach (var diagnostic in result.GetFeature<Diagnostics>())
                        {
                            console.Out.WriteLine($"\t\t{diagnostic.Message}");
                        }
                    }
                }
            }

            void ReportCodeLinkageResults(CodeLinkBlock codeLinkBlock)
            {
                var diagnostics = codeLinkBlock.Diagnostics.ToArray();

                Console.ResetColor();

                console.Out.WriteLine("  Checking Markdown...");

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