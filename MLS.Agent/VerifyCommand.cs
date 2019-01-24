using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MLS.Agent.Markdown;

namespace MLS.Agent
{
    public static class VerifyCommand
    {
        public static Task<int> Do(
            DirectoryInfo rootDirectory,
            IConsole console,
            Func<IDirectoryAccessor> getDirectoryAccessor)
        {
            var directoryAccessor = getDirectoryAccessor();
            var markdownProject = new MarkdownProject(directoryAccessor);
            var returnCode = 0;

            foreach (var markdownFile in markdownProject.GetAllMarkdownFiles())
            {
                var fullName = directoryAccessor.GetFullyQualifiedPath(markdownFile.Path).FullName;

                console.Out.WriteLine(fullName);

                var codeLinkBlocks = markdownFile.GetCodeLinkBlocks();

                foreach (var codeLinkBlock in codeLinkBlocks)
                {
                    var diagnostics = codeLinkBlock.Diagnostics.ToArray();

                    if (diagnostics.Any())
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        returnCode = 1;
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

                    Console.ResetColor();
                }
            }

            return Task.FromResult(returnCode);
        }
    }
}