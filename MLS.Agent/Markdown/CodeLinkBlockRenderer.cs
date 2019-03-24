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
                const string errorSvg = "<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" version=\"1.1\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\"><path d=\"M23,12L20.56,14.78L20.9,18.46L17.29,19.28L15.4,22.46L12,21L8.6,22.47L6.71,19.29L3.1,18.47L3.44,14.78L1,12L3.44,9.21L3.1,5.53L6.71,4.72L8.6,1.54L12,3L15.4,1.54L17.29,4.72L20.9,5.54L20.56,9.22L23,12M20.33,12L18.5,9.89L18.74,7.1L16,6.5L14.58,4.07L12,5.18L9.42,4.07L8,6.5L5.26,7.09L5.5,9.88L3.67,12L5.5,14.1L5.26,16.9L8,17.5L9.42,19.93L12,18.81L14.58,19.92L16,17.5L18.74,16.89L18.5,14.1L20.33,12M11,15H13V17H11V15M11,7H13V13H11V7\" /></svg>";
                renderer.WriteLine(@"<div class=""notification is-danger"">");
                renderer.WriteLine(errorSvg);

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
                .WriteLine(">")
                .WriteEscape(codeLinkBlock.Lines.ToSlice().ToString())
                .WriteLine()
                .WriteLine(@"</code>")
                .WriteLine(@"</pre>")
                .WriteLineIf(!codeLinkBlock.IsInclude, @"</div >");

            if (InlineControls && !codeLinkBlock.IsInclude)
            {
                //const string playSvg = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 9 10\"><path fill=\"white\" d=\"M1,0 1,10, 9,5z\" /></svg>";

                const string playSvg = "<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" version=\"1.1\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\"><path d=\"M8,5.14V19.14L19,12.14L8,5.14Z\" /></svg>";

                renderer
                    .WriteLine($@"<button class=""run"" data-trydotnet-mode=""run"" data-trydotnet-session-id=""{codeLinkBlock.Session}"" data-trydotnet-run-args=""{codeLinkBlock.RunArgs.HtmlAttributeEncode()}"">{playSvg}</button>");

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