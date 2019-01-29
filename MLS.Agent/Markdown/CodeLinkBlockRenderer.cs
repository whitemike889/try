using System;
using System.Linq;
using Markdig.Helpers;
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

            if (codeLinkBlock.Diagnostics.Any())
            {
                renderer.WriteLine(@"<div class=""notification is-danger"">");
                renderer.WriteLine(@"<span class=""icon is-small""><i class=""error-icon""></i></span>");
                
                foreach (var diagnostic in codeLinkBlock.Diagnostics)
                {
                    renderer.WriteEscape("\t"+diagnostic);
                    renderer.WriteLine();
                }

                renderer.WriteLine(@"</div>");

                return;
            }

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
            var size = (text.ToString().Split("\n").Length + 3) * 20;
            return Math.Max(100, size);
        }
    }
}