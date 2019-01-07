using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlock: FencedCodeBlock
    {
        public StringSlice CodeLines { get; set; }
        public string ExceptionMessage { get; set; }

        public CodeLinkBlock(BlockParser parser): base(parser)
        {
        }
    }
}
