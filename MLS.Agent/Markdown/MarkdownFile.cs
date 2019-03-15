using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Markdig;
using Microsoft.AspNetCore.Html;

namespace MLS.Agent.Markdown
{
    public class MarkdownFile
    {
        public MarkdownFile(
            RelativeFilePath path,
            MarkdownProject project)
        {
            Path = path;
            Project = project;
        }

        public RelativeFilePath Path { get; }

        public MarkdownProject Project { get; }

        public async Task<IEnumerable<CodeLinkBlock>> GetCodeLinkBlocks()
        {
            var pipeline = Project.GetMarkdownPipelineFor(Path);

            var document = Markdig.Markdown.Parse(
                ReadAllText(),
                pipeline);

            var blocks = document.OfType<CodeLinkBlock>().ToList();

            await Task.WhenAll(blocks.Select(b => b.InitializeAsync()));
            return blocks;
        }

        public async Task<IHtmlContent> ToHtmlContentAsync()
        {
            var pipeline = Project.GetMarkdownPipelineFor(Path);
            var extension = pipeline.Extensions.FindExact<CodeLinkExtension>();
            if (extension != null)
            {
                var blocks = await GetCodeLinkBlocks();
                var maxEditorPerSession = blocks
                    .GroupBy(b => b.Session)
                    .Max(editors => editors.Count());

                extension.InlineControls = maxEditorPerSession <= 1;
            }

            var html = await pipeline.RenderHtmlAsync(ReadAllText());
            return new HtmlString(html);
        }

        public string ReadAllText() =>
            Project.DirectoryAccessor.ReadAllText(Path);
    }
}