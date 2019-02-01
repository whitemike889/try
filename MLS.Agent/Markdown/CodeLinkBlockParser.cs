using System;
using System.Threading.Tasks;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using WorkspaceServer;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlockParser : FencedBlockParserBase<CodeLinkBlock>
    {
        private readonly MarkdownArgumentParser _csharpLinkParser;
        private readonly IDirectoryAccessor _directoryAccessor;

        private readonly PackageRegistry _registry;

        public CodeLinkBlockParser(IDirectoryAccessor directoryAccessor, PackageRegistry registry)
        {
            OpeningCharacters = new[] { '`' };
            InfoParser = ParseCodeOptions;
            _directoryAccessor = directoryAccessor ?? throw new ArgumentNullException(nameof(directoryAccessor));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _csharpLinkParser = new MarkdownArgumentParser(_directoryAccessor);
        }

        protected override CodeLinkBlock CreateFencedBlock(BlockProcessor processor) =>
            new CodeLinkBlock(this);

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

            if (parseResult == null)
            {
                return false;
            }

            codeLinkBlock.AddOptions(parseResult, () => GetAccessor(parseResult.Package, _registry, _directoryAccessor));

            return true;
        }

        private static async Task<IDirectoryAccessor> GetAccessor(string package, PackageRegistry registry, IDirectoryAccessor defaultAccessor)
        {
            if (package != null)
            {
                var installedPackage = await registry.Get(package);
                if (installedPackage != null && installedPackage.Directory != null)
                {
                    return new FileSystemDirectoryAccessor(installedPackage.Directory);
                }
            }

            return defaultAccessor;
        }

        public override BlockState TryContinue(BlockProcessor processor, Block block)
        {
            var fence = (IFencedBlock)block;
            var count = fence.FencedCharCount;
            var matchChar = fence.FencedChar;
            var c = processor.CurrentChar;

            // Match if we have a closing fence
            var line = processor.Line;
            while (c == matchChar)
            {
                c = line.NextChar();
                count--;
            }

            // If we have a closing fence, close it and discard the current line
            // The line must contain only fence opening character followed only by whitespaces.
            if (count <= 0 && !processor.IsCodeIndent && (c == '\0' || c.IsWhitespace()) && line.TrimEnd())
            {
                block.UpdateSpanEnd(line.Start - 1);

                // Don't keep the last line
                return BlockState.BreakDiscard;
            }

            // Reset the indentation to the column before the indent
            processor.GoToColumn(processor.ColumnBeforeIndent);

            var codeBlock = block as CodeLinkBlock;

            //if we already have the source code discard the lines that are inside the fenced code
            if (codeBlock != null && codeBlock.SourceFile != null)
            {
                return BlockState.ContinueDiscard;
            }

            return BlockState.Continue;
        }
    }
}