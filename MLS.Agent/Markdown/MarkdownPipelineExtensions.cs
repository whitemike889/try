using Markdig;
using Markdig.Renderers;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MLS.Agent.Markdown
{
    public static class MarkdownPipelineExtensions
    {
        public static async Task<string> ToHtmlAsync(this MarkdownPipeline pipeline, string text)
        {
            var document = Markdig.Markdown.Parse(
               text,
               pipeline);

            foreach (var codeLinkBlock in document.OfType<CodeLinkBlock>())
            {
                await codeLinkBlock.InitializeAsync();
            }

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
