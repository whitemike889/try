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

            if (codeLinkBlock.ErrorMessage.Any())
            {
                foreach (var message in codeLinkBlock.ErrorMessage)
                {
                    renderer.WriteEscape(message);
                    renderer.WriteLine();
                }
                
                return;
            }

            var session = codeLinkBlock.GetAttributes().Properties.FirstOrDefault(p => p.Key == "data-trydotnet-session-id").Value;

            renderer
                .WriteLine(@"<div class=""editor-panel"">")
                .WriteLine($@"<pre style=""border:none"" height=""{GetEditorHeight(codeLinkBlock.CodeLines)}em"" width=""100%"">")
                .Write("<code")
                .WriteAttributes(codeLinkBlock)
                .WriteLine(">")
                .WriteEscape(codeLinkBlock.CodeLines)
                .WriteLine()
                .WriteLine("</code>")
                .WriteLine("</pre>")
                .WriteLine(@"</div >");


            AddRunButtonForSession(renderer, session);

            AddOutputForSession(renderer, session);
        }

        private static int GetEditorHeight(StringSlice text)
        {
            var height = 
            (text.Text.Split(Environment.NewLine)?.Length ?? 0) * 20;
            return height;
        }

        private static void AddOutputForSession(HtmlRenderer renderer, string session)
        {
            renderer.Write(@"<div class=""output-panel"" data-trydotnet-mode=""runResult""");
            WriteDotnetSessionAttribute(renderer, session);
            renderer.WriteLine(@"></div>");
        }

        private static void WriteDotnetSessionAttribute(HtmlRenderer renderer, string session)
        {
            if (!string.IsNullOrWhiteSpace(session))
            {
                renderer.Write($@" data-trydotnet-session-id=""{session}""");
            }
        }

        private static void AddRunButtonForSession(HtmlRenderer renderer, string session)
        {
            var buttonLabel = string.IsNullOrWhiteSpace(session) ? "Run" : $"Run {session}";
            renderer.Write(@"<button class=""run-button"" data-trydotnet-mode=""run""");
            WriteDotnetSessionAttribute(renderer, session);
            renderer.WriteLine($@">{buttonLabel}</button>");
        }
    }
}