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
                    var sourceFilePath = codeLinkBlock.SourceFile;

                    var sourceFile =
                        sourceFilePath != null
                            ? directoryAccessor.GetFullyQualifiedPath(sourceFilePath).FullName
                            : "UNKNOWN";

                    var projectPath = codeLinkBlock.ProjectFile;

                    var project = projectPath != null
                                      ? projectPath.FullName
                                      : "UNKNOWN";

                    console.Out.WriteLine($"  {sourceFile} (in project {project})");

                    var diagnostics = codeLinkBlock.Diagnostics.ToArray();

                    if (diagnostics.Any())
                    {
                        foreach (var diagnostic in diagnostics)
                        {
                            console.Out.WriteLine($"  ! {codeLinkBlock.MarkdownFile} (line {codeLinkBlock.Line}): {diagnostic.Message}");
                        }

                        returnCode = 1;
                    }
                }
            }

            return Task.FromResult(returnCode);
        }
    }
}