using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlockRenderer : CodeBlockRenderer
    {
        protected override void Write(
            HtmlRenderer renderer,
            CodeBlock codeBlock)
        {
            var parser = codeBlock.Parser as CodeLinkBlockParser;
            if (!(codeBlock is CodeLinkBlock codeLinkBlock) || parser == null)
            {
                base.Write(renderer, codeBlock);
                return;
            }

            if (codeLinkBlock.ErrorMessage != null)
            {
                renderer.WriteEscape(codeLinkBlock.ErrorMessage);
                return;
            }

            renderer
                .WriteLine(@"<pre style=""border:none"" height=""300px"" width=""800px"">")
                .Write("<code")
                .WriteAttributes(codeLinkBlock)
                .WriteLine(">")
                .WriteEscape(codeLinkBlock.CodeLines)
                .WriteLine()
                .WriteLine("</code></pre>")
                .WriteLine(@"<button data-trydotnet-mode=""run"" data-trydotnet-session-id=""a"">Run</button>")
                .WriteLine(@"<div data-trydotnet-mode=""runResult"" data-trydotnet-session-id=""a""></div>");
        }
    }
}