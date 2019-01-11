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

            //to do: ask what are the config objects that will be required here like projectTemplate, trydotnet mode, the url to do auto enable, etc
            renderer
                .WriteLine(
                    @"<pre style=""border: none"" height=""300px"" width=""800px"" data-trydotnet-mode=""editor"" data-trydotnet-project-template=""testmagic"" data-trydotnet-session-id=""a"" height=""300px"" width=""800px"">")
                .WriteEscape(codeLinkBlock.CodeLines)
                .WriteLine()
                .WriteLine(@"</pre>")
                .WriteLine(@"<button data-trydotnet-mode=""run"" data-trydotnet-session-id=""a"">Run</button>")
                .WriteLine(@"<div data-trydotnet-mode=""runResult"" data-trydotnet-session-id=""a""></div>");
        }
    }
}