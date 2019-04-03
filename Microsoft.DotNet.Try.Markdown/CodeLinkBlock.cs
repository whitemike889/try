using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Microsoft.DotNet.Try.Markdown
{
    public class CodeLinkBlock : FencedCodeBlock
    {
        protected readonly List<string> _diagnostics = new List<string>();
        private string _sourceCode;
        private bool _initialized;

        public CodeLinkBlock(
            BlockParser parser,
            int order = 0) : base(parser)
        {
            Order = order;
        }

        public IList<string> Diagnostics => _diagnostics;

        public CodeLinkBlockOptions Options { get; set; }

        public int Order { get; }

        public virtual async Task InitializeAsync()
        {
            if (Options == null && Diagnostics.Count == 0)
            {
                throw new InvalidOperationException("Attempted to initialize block before parsing code fence options");
            }

            if (_initialized)
            {
                return;
            }

            _initialized = true;

            if (Options != null)
            {
                var result = await Options.TryGetExternalContent();

                _diagnostics.AddRange(result.ErrorMessages);

                if (!Diagnostics.Any())
                {
                    if (result.Content != null)
                    {
                        SourceCode = result.Content;
                    }
                    else
                    {
                        SourceCode = Lines.ToString();
                    }

                    await AddAttributes(Options);
                }
            }
        }

        private void AddAttributeIfNotNull(string name, object value)
        {
            if (value != null)
            {
                AddAttribute(name, value.ToString());
            }
        }

        protected virtual async Task AddAttributes(CodeLinkBlockOptions options)
        {
            await options.AddAttributes(this);

            AddAttribute("data-trydotnet-order", Order.ToString("F0"));

            AddAttribute("data-trydotnet-mode", options.Editable ? "editor" : "include");

            if (options.Hidden)
            {
                AddAttribute("data-trydotnet-visibility", "hidden");
            }

            AddAttributeIfNotNull("data-trydotnet-region", options.Region);
            AddAttributeIfNotNull("data-trydotnet-session-id", options.Session);
            AddAttribute("class", $"language-{options.Language}");
        }

        public void RenderTo(
            HtmlRenderer renderer,
            bool InlineControls,
            bool EnablePreviewFeatures)
        {
            var height = $"{GetEditorHeightInEm(Lines)}em";

            if (Options.Editable)
            {
                renderer
                    .WriteLine(InlineControls
                                   ? @"<div class=""inline-code-container"">"
                                   : @"<div class=""code-container"">");
            }

            renderer
                .WriteLineIf(Options.Editable, @"<div class=""editor-panel"">")
                .WriteLine(Options.Editable
                               ? $@"<pre style=""border:none; height: {height}"" height=""{height}"" width=""100%"">"
                               : @"<pre>")
                .Write("<code")
                .WriteAttributes(this)
                .Write(">")
                .WriteLeafRawLines(this, true, true)
                .Write(@"</code>")
                .WriteLine(@"</pre>")
                .WriteLineIf(Options.Editable, @"</div >");

            if (InlineControls && Options.Editable)
            {
                renderer
                    .WriteLine(
                        $@"<button class=""run"" data-trydotnet-mode=""run"" data-trydotnet-session-id=""{Options.Session}"" data-trydotnet-run-args=""{Options.RunArgs.HtmlAttributeEncode()}"">{SvgResources.PlaySvg}</button>");

                renderer
                    .WriteLine(EnablePreviewFeatures
                                   ? $@"<div class=""output-panel-inline collapsed"" data-trydotnet-mode=""runResult"" data-trydotnet-output-type=""terminal"" data-trydotnet-session-id=""{Options.Session}""></div>"
                                   : $@"<div class=""output-panel-inline collapsed"" data-trydotnet-mode=""runResult"" data-trydotnet-session-id=""{Options.Session}""></div>");
            }

            if (Options.Editable)
            {
                renderer.WriteLine("</div>");
            }

            int GetEditorHeightInEm(StringLineGroup text)
            {
                var size = text.ToString().Split('\n').Length + 6;
                return Math.Max(8, size);
            }
        }

        public string SourceCode
        {
            get => _sourceCode;

            set
            {
                _sourceCode = value ?? "";
                Lines = new StringLineGroup(_sourceCode);
            }
        }

        public void AddAttribute(string key, string value)
        {
            this.GetAttributes().AddProperty(key, value);
        }
    }
}