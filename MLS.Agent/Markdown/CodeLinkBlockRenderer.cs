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

        public CodeLinkBlockRenderer()
        {
            OutputAttributesOnPre = false;
        }

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
                renderer.WriteLine(SvgResources.ErrorSvg);

                foreach (var diagnostic in codeLinkBlock.Diagnostics)
                {
                    renderer.WriteEscape("\t" + diagnostic);
                    renderer.WriteLine();
                }

                renderer.WriteLine(@"</div>");

                return;
            }

            var height = $"{GetEditorHeightInEm(codeLinkBlock.Lines)}em";

            if (!codeLinkBlock.IsInclude)
            {
                renderer
                    .WriteLine(InlineControls
                                   ? @"<div class=""inline-code-container"">"
                                   : @"<div class=""code-container"">");
            }

            renderer
                .WriteLineIf(!codeLinkBlock.IsInclude, @"<div class=""editor-panel"">")
                .WriteLine(codeLinkBlock.IsInclude
                               ? @"<pre>"
                               : $@"<pre style=""border:none; height: {height}"" height=""{height}"" width=""100%"">")
                .Write("<code")
                .WriteAttributes(codeLinkBlock)
                .Write(">")
                .WriteLeafRawLines(codeLinkBlock, true,true)
                .Write(@"</code>")
                .WriteLine(@"</pre>")
                .WriteLineIf(!codeLinkBlock.IsInclude, @"</div >");

            if (InlineControls && !codeLinkBlock.IsInclude)
            {
              
                renderer
                    .WriteLine($@"<button class=""run"" data-trydotnet-mode=""run"" data-trydotnet-session-id=""{codeLinkBlock.Session}"" data-trydotnet-run-args=""{codeLinkBlock.RunArgs.HtmlAttributeEncode()}"">{SvgResources.PlaySvg}</button>");

                renderer
                    .WriteLine(EnablePreviewFeatures
                        ? $@"<div class=""output-panel-inline collapsed"" data-trydotnet-mode=""runResult"" data-trydotnet-output-type=""terminal"" data-trydotnet-session-id=""{codeLinkBlock.Session}""></div>"
                        : $@"<div class=""output-panel-inline collapsed"" data-trydotnet-mode=""runResult"" data-trydotnet-session-id=""{codeLinkBlock.Session}""></div>");
            }

            if (!codeLinkBlock.IsInclude)
            {
                renderer.WriteLine("</div>");
            }
        }

        public bool EnablePreviewFeatures { get; set; }

        private static int GetEditorHeightInEm(StringLineGroup text)
        {
            var size = (text.ToString().Split("\n").Length + 6);
            return Math.Max(8, size);
        }
    }

    internal static class TextRendererBaseExtensions
    {
        public static T WriteLineIf<T>(this T textRendererBase, bool @if, string value)
            where T : HtmlRenderer
        {
            if (@if)
            {
                textRendererBase.WriteLine(value);
            }

            return textRendererBase;
        }
    }
}