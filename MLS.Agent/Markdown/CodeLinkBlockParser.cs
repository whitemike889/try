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
        private readonly Parser _csharpLinkParser;

        private readonly IDirectoryAccessor _directoryAccessor;

        public CodeLinkBlockParser(IDirectoryAccessor directoryAccessor)
        {
            OpeningCharacters = new[] { '`' };
            InfoParser = ParseCodeOptions;
            _directoryAccessor = directoryAccessor ?? throw new ArgumentNullException(nameof(directoryAccessor));
            _csharpLinkParser = CodeLinkBlock.CreateOptionsParser(_directoryAccessor);
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

            if (parseResult.CommandResult.Name != "csharp")
            {
                return false;
            }

            codeLinkBlock.Region = parseResult.ValueForOption<string>("region");

            codeLinkBlock.Session = parseResult.ValueForOption<string>("session");

            if (parseResult.CommandResult.Result is SuccessfulArgumentParseResult)
            {
                codeLinkBlock.SourceFile = parseResult.CommandResult.GetValueOrDefault<RelativeFilePath>();
            }

            if (codeLinkBlock.SourceFile != null)
            {
                codeLinkBlock.Lines = new StringLineGroup(codeLinkBlock.SourceCode);
            }

            if (parseResult.Errors.Any())
            {
                var errors = parseResult.Errors.Select(e => e.ToString());

                foreach (var error in errors)
                {
                    codeLinkBlock.AddDiagnostic(error);
                }
            }

            var optionResult = parseResult.CommandResult["project"];
            if (optionResult?.Result is SuccessfulArgumentParseResult)
            {
                codeLinkBlock.ProjectFile = parseResult.CommandResult.ValueForOption<FileInfo>("project");

                var packageName = GetPackageNameFromProjectFile(codeLinkBlock.ProjectFile);

                if (packageName == null &&
                    codeLinkBlock.SourceFile != null)
                {
                    codeLinkBlock.AddDiagnostic(
                        $"No project file could be found at path {_directoryAccessor.GetFullyQualifiedPath(new RelativeDirectoryPath("."))}");
                }
            }

            return true;
        }

        private static string GetPackageNameFromProjectFile(FileInfo projectFile)
        {
            return projectFile?.FullName;
        }
    }
}