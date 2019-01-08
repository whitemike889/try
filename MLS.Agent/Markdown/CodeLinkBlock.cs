using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlock : FencedCodeBlock
    {
        public StringSlice CodeLines { get; set; }

        public string ErrorMessage { get; set; }

        public CodeLinkBlock(BlockParser parser) : base(parser)
        {
        }
    }
}