using System;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using System.Linq;
using Markdig.Helpers;

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

            if (codeLinkBlock.Diagnostics.Any())
            {
                foreach (var diagnostic in codeLinkBlock.Diagnostics)
                {
                    renderer.WriteEscape(diagnostic.Message);
                    renderer.WriteLine();
                }

                return;
            }

            var session = codeLinkBlock.Session;

            renderer
                .WriteLine(@"<div class=""editor-panel"">")
                .WriteLine($@"<pre style=""border:none"" height=""{GetEditorHeightInEm(codeLinkBlock.Lines)}em"" width=""100%"">")
                .Write("<code")
                .WriteAttributes(codeLinkBlock)
                .WriteLine(">")
                .WriteEscape(codeLinkBlock.Lines.ToSlice().ToString())
                .WriteLine()
                .WriteLine(@"</code>")
                .WriteLine(@"</pre>")
                .WriteLine(@"</div >");
        }

        private static int GetEditorHeightInEm(StringLineGroup text)
        {
            return (text.ToString().Split("\n").Length + 3) * 50;
        }
    }
}