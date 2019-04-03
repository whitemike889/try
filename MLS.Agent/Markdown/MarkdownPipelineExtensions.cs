using Markdig;
using Markdig.Renderers;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.Try.Markdown;

namespace MLS.Agent.Markdown
{
    public static class MarkdownPipelineExtensions
    {
        public static async Task<string> RenderHtmlAsync(this MarkdownPipeline pipeline, string text)
        {
            // FIX: (RenderHtmlAsync)  CodeLinkBlockParser.ResetOrder();

            var document = Markdig.Markdown.Parse(
               text,
               pipeline);

            var initializeTasks = document.OfType<CodeLinkBlock>()
                .Select(c => c.InitializeAsync());

            await Task.WhenAll(initializeTasks);

            using (var writer = new StringWriter())
            {
                var renderer = new HtmlRenderer(writer);
                pipeline.Setup(renderer);
                renderer.Render(document);
                var html = writer.ToString();
                return html;
            }
        }
    }
}
