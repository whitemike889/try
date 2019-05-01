using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.Try.Markdown;
using Microsoft.DotNet.Try.Project;
using Microsoft.DotNet.Try.Protocol;
using MLS.Agent.Markdown;
using WorkspaceServer;
using WorkspaceServer.Servers.Roslyn;
using Buffer = Microsoft.DotNet.Try.Protocol.Buffer;

namespace MLS.Agent.CommandLine
{
    public static class VerifyCommand
    {
        public static async Task<int> Do(
            VerifyOptions options,
            IConsole console,
            Func<IDirectoryAccessor> getDirectoryAccessor,
            PackageRegistry packageRegistry,
            StartupOptions startupOptions = null)
        {
            var directoryAccessor = getDirectoryAccessor();
            var markdownProject = new MarkdownProject(
                directoryAccessor,
                packageRegistry,
                startupOptions);
            var returnCode = 0;
            var workspaceServer = new Lazy<RoslynWorkspaceServer>(() => new RoslynWorkspaceServer(packageRegistry));

            var markdownFiles = markdownProject.GetAllMarkdownFiles().ToArray();

            if (markdownFiles.Length == 0)
            {
                console.Error.Write($"No markdown files found under {options.Dir}");
                return -1;
            }

            foreach (var markdownFile in markdownFiles)
            {
                var fullName = directoryAccessor.GetFullyQualifiedPath(markdownFile.Path).FullName;
                var markdownFileDirectoryAccessor = directoryAccessor.GetDirectoryAccessorForRelativePath(markdownFile.Path.Directory);

                console.Out.WriteLine();
                console.Out.WriteLine(fullName);
                console.Out.WriteLine(new string('-', fullName.Length));

                var codeLinkBlocks = await markdownFile.GetAnnotatedCodeBlocks();

                var sessions = codeLinkBlocks.GroupBy(block => block.Annotations?.Session);

                foreach (var session in sessions)
                {
                    if (session.Select(block => block.ProjectOrPackageName()).Distinct().Count() != 1)
                    {
                        SetError();
                        console.Out.WriteLine($"Session cannot span projects or packages: --session {session.Key}");
                        continue;
                    }

                    foreach (var codeLinkBlock in session)
                    {
                        ReportCodeLinkageResults(codeLinkBlock, markdownFileDirectoryAccessor);
                    }

                    Console.ResetColor();

                    if (!session.Any(block => block.Diagnostics.Any()))
                    {
                        var (buffersToInclude, filesToInclude) = await markdownFile.GetIncludes(markdownFileDirectoryAccessor);
                        await ReportCompileResults(session, markdownFile, filesToInclude, buffersToInclude, markdownFileDirectoryAccessor);
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

            async Task ReportCompileResults(IGrouping<string, AnnotatedCodeBlock> session, MarkdownFile markdownFile, Dictionary<string, File[]> filesToInclude,
                IReadOnlyDictionary<string, Buffer[]> buffersToInclude, IDirectoryAccessor accessor)
            {
                console.Out.WriteLine($"\n  Compiling samples for session \"{session.Key}\"\n");

                var editableCodeBlocks = session.Where(b => b.Annotations.Editable).ToList();

                var projectOrPackageName = session
                    .Select(b => b.ProjectOrPackageName())
                    .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));

                var buffers = editableCodeBlocks
                              .Select(block => block.GetBufferAsync(accessor, markdownFile))
                              .ToList();

                var files = new List<File>();

                if (filesToInclude.TryGetValue("global", out var globalIncludes))
                {
                    files.AddRange(globalIncludes);
                }

                if (!string.IsNullOrWhiteSpace(session.Key) && filesToInclude.TryGetValue(session.Key, out var sessionIncludes))
                {
                    files.AddRange(sessionIncludes);
                }

                if (buffersToInclude.TryGetValue("global", out var globalSessionBuffersToInclude))
                {
                    buffers.AddRange(globalSessionBuffersToInclude);
                }

                if (!string.IsNullOrWhiteSpace(session.Key) && buffersToInclude.TryGetValue(session.Key, out var localSessionBuffersToInclude))
                {
                    buffers.AddRange(localSessionBuffersToInclude);
                }

                var workspace = new Workspace(
                    workspaceType: projectOrPackageName,
                    files: files.ToArray(),
                    buffers: buffers.ToArray());

                var mergeTransformer = new CodeMergeTransformer();
                var inliningTransformer = new BufferInliningTransformer();

                var processed = await mergeTransformer.TransformAsync(workspace);
                processed = await inliningTransformer.TransformAsync(processed);
                processed = new Workspace(usings: processed.Usings, workspaceType: processed.WorkspaceType, files: processed.Files);

                var result = await workspaceServer.Value.Compile(new WorkspaceRequest(processed));

                var projectDiagnostics = result.GetFeature<ProjectDiagnostics>()
                                               .Where(e => e.Severity == DiagnosticSeverity.Error)
                                               .ToArray();
                if (projectDiagnostics.Any())
                {
                    SetError();

                    console.Out.WriteLine($"    Build failed for project {projectOrPackageName}");

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

            void ReportCodeLinkageResults(AnnotatedCodeBlock codeLinkBlock, IDirectoryAccessor accessor)
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

                var blockOptions = (LocalCodeBlockAnnotations)codeLinkBlock.Annotations;

                var file = blockOptions?.SourceFile ?? blockOptions?.DestinationFile;
                var sourceFile =
                    file != null
                        ? accessor.GetFullyQualifiedPath(file).FullName
                        : "UNKNOWN";

                var project = codeLinkBlock.ProjectOrPackageName() ?? "UNKNOWN";

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