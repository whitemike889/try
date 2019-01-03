using Markdig.Helpers;
using Markdig.Syntax;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlock: FencedCodeBlock
    {
        public StringSlice CodeLines { get; set; }
        public string ExceptionMessage { get; set; }

        public CodeLinkBlock(CodeLinkBlockParser parser): base(parser)
        {
        }
    }
}
