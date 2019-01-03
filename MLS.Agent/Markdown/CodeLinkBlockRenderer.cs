using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlockRenderer : CodeBlockRenderer
    {
        private readonly Configuration _config;

        public CodeLinkBlockRenderer(Configuration config)
        {
            _config = config;
        }

        protected override void Write(HtmlRenderer renderer, CodeBlock obj)
        {
            var parser = obj.Parser as CodeLinkBlockParser;
            if (!(obj is CodeLinkBlock codeLinkBlock) || parser == null)
            {
                base.Write(renderer, obj);
                return;
            }

            if (codeLinkBlock.ExceptionMessage != null)
            {
                renderer.WriteEscape(codeLinkBlock.ExceptionMessage);
                return;
            }

            //to do: ask what are the config objects that will be required here like projectTemplate, trydotnet mode, the url to do auto enable, etc
            renderer.Write(@"<script src=""//trydotnet.microsoft.com/api/trydotnet.min.js""></script>");
            renderer.Write(@"<pre style=""border: none"" height=""300px"" width=""800px"" trydotnetMode=""editor"" projectTemplate=""console"" trydotnetSessionId=""a"" height=""300px"" width=""800px"">");
            renderer.WriteEscape(codeLinkBlock.CodeLines);
            renderer.WriteLine();
            renderer.Write("</pre>");
            renderer.Write(@"<script nonce=""3Ylwe7FVSanYwwVoBKXA1WLjbN8vnTKFyv90yityOU4="" >trydotnet.autoEnable(new URL(""https://localhost:5001/""));</script>");
        }
    }
}