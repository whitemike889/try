using System;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Microsoft.DotNet.Try.Markdown
{
    public class CodeLinkBlockParser : FencedBlockParserBase<CodeLinkBlock>
    {
        private readonly CodeFenceOptionsParser _codeFenceOptionsParser;
        private int _order;

        public CodeLinkBlockParser(CodeFenceOptionsParser codeFenceOptionsParser)
        {
            _codeFenceOptionsParser = codeFenceOptionsParser ?? throw new ArgumentNullException(nameof(codeFenceOptionsParser));
            OpeningCharacters = new[] { '`' };
            InfoParser = ParseCodeOptions;
        }

        protected override CodeLinkBlock CreateFencedBlock(BlockProcessor processor) =>
            new CodeLinkBlock(this, _order++);

        protected bool ParseCodeOptions(
            BlockProcessor state,
            ref StringSlice line,
            IFencedBlock fenced)
        {
            if (!(fenced is CodeLinkBlock codeLinkBlock))
            {
                return false;
            }

            var result = _codeFenceOptionsParser.TryParseCodeFenceOptions(
                line.ToString());

            switch (result)
            {
                case NoCodeFenceOptions _:
                    return false;
                case FailedCodeFenceOptionParseResult failed:
                    foreach (var errorMessage in failed.ErrorMessages)
                    {
                        codeLinkBlock.Diagnostics.Add(errorMessage);
                    }

                    break;
                case SuccessfulCodeFenceOptionParseResult successful:
                    codeLinkBlock.Options = successful.Options;
                    break;
            }

            return true;
        }

        public override BlockState TryContinue(
            BlockProcessor processor,
            Block block)
        {
            var fence = (IFencedBlock) block;
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

            // FIX: (TryContinue) 
            // if we already have the source code discard the lines that are inside the fenced code
            // if (codeBlock?.Options is LocalCodeLinkBlockOptions localOptions &&
            //     localOptions.SourceFile != null)
            // {
            //     return BlockState.ContinueDiscard;
            // }

            return BlockState.Continue;
        }
    }
}