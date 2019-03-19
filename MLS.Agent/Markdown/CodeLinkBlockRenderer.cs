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
        public bool InlineControls { get; set; }

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
                    renderer.WriteEscape("\t" + diagnostic);
                    renderer.WriteLine();
                }

                renderer.WriteLine(@"</div>");

                return;
            }

            var height = $"{GetEditorHeightInEm(codeLinkBlock.Lines)}em";

            renderer
                .WriteLine(InlineControls
                    ? @"<div class=""inline-code-container"">" 
                    : @"<div class=""code-container"">");

            renderer
                .WriteLine($@"<div class=""editor-panel"">")
                .WriteLine($@"<pre style=""border:none; height: {height}"" height=""{height}"" width=""100%"">")
                .Write("<code")
                .WriteAttributes(codeLinkBlock)
                .WriteLine(">")
                .WriteEscape(codeLinkBlock.Lines.ToSlice().ToString())
                .WriteLine()
                .WriteLine(@"</code>")
                .WriteLine(@"</pre>")
                .WriteLine(@"</div >");

            if (InlineControls)
            {
                const string playSvg = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 9 10\"><path fill=\"white\" d=\"M1,0 1,10, 9,5z\" /></svg>";

                renderer
                    .WriteLine($@"<button class=""run"" data-trydotnet-mode=""run"" data-trydotnet-session-id=""{codeLinkBlock.Session}"" data-trydotnet-run-args=""{codeLinkBlock.RunArgs.HtmlAttributeEncode()}"">{playSvg}</button>");

                renderer
                    .WriteLine(EnablePreviewFeatures
                        ? $@"<div class=""output-panel-inline collapsed"" data-trydotnet-mode=""runResult"" data-trydotnet-output-type=""terminal"" data-trydotnet-session-id=""{codeLinkBlock.Session}""></div>"
                        : $@"<div class=""output-panel-inline collapsed"" data-trydotnet-mode=""runResult"" data-trydotnet-session-id=""{codeLinkBlock.Session}""></div>");
            }

            renderer.WriteLine("</div>");
        }

        public bool EnablePreviewFeatures { get; set; }

        private static int GetEditorHeightInEm(StringLineGroup text)
        {
            var size = (text.ToString().Split("\n").Length + 6);
            return Math.Max(8, size);
        }
    }
}