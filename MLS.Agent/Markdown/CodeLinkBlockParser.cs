using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlockParser : FencedBlockParserBase<CodeLinkBlock>
    {
        private readonly MarkdownArgumentParser _csharpLinkParser;

        private readonly IDirectoryAccessor _directoryAccessor;

        public CodeLinkBlockParser(IDirectoryAccessor directoryAccessor)
        {
            OpeningCharacters = new[] { '`' };
            InfoParser = ParseCodeOptions;
            _directoryAccessor = directoryAccessor ?? throw new ArgumentNullException(nameof(directoryAccessor));
            _csharpLinkParser = new MarkdownArgumentParser(_directoryAccessor);
        }

        protected override CodeLinkBlock CreateFencedBlock(BlockProcessor processor) => 
            new CodeLinkBlock(this, _directoryAccessor);

        private bool ParseCodeOptions(
            BlockProcessor state,
            ref StringSlice line,
            IFencedBlock fenced)
        {
            var codeLinkBlock = fenced as CodeLinkBlock;

            if (codeLinkBlock == null)
            {
                return false;
            }

            var parseResult = _csharpLinkParser.Parse(line.ToString());

            if (parseResult ==  null)
            {
                return false;
            }

            codeLinkBlock.Region = parseResult.Region;

            codeLinkBlock.Session = parseResult.Session;

            codeLinkBlock.SourceFile = parseResult.SourceFile;

            if (codeLinkBlock.SourceFile != null)
            {
                codeLinkBlock.Lines = new StringLineGroup(codeLinkBlock.SourceCode);
            }

            if (parseResult.Errors.Any())
            {
                foreach (var error in parseResult.Errors)
                {
                    codeLinkBlock.AddDiagnostic(error);
                }
            }

            var project = parseResult.Project;
            var package = parseResult.Package;


            if (!string.IsNullOrWhiteSpace(package))
            {
                codeLinkBlock.Package = parseResult.Package;
            }

            if (codeLinkBlock.Package == null &&  project != null)
            {
                codeLinkBlock.ProjectFile = project;

                var packageName = GetPackageNameFromProjectFile(codeLinkBlock.ProjectFile);

                if (packageName == null &&
                    codeLinkBlock.SourceFile != null)
                {
                    codeLinkBlock.AddDiagnostic(
                        $"No project file could be found at path {_directoryAccessor.GetFullyQualifiedPath(new RelativeDirectoryPath("."))}");
                }
            }

            if (codeLinkBlock.Package != null && !parseResult.IsProjectImplicit)
            {
                codeLinkBlock.AddDiagnostic("Can't specify both --project and --package");
            }

            return true;
        }

        private static string GetPackageNameFromProjectFile(FileInfo projectFile)
        {
            return projectFile?.FullName;
        }
    }
}