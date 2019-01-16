using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using System.Collections.Generic;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlock : FencedCodeBlock
    {
        public StringSlice CodeLines { get; set; }

        public List<string> ErrorMessage { get; }

        public CodeLinkBlock(BlockParser parser) : base(parser)
        {
            ErrorMessage = new List<string>();
        }

        public void AddAttribute(string key, string value)
        {
            this.GetAttributes().AddProperty(key, value);
        }
    }
}