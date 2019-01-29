using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Markdig.Renderers;
using Microsoft.AspNetCore.Html;
using MLS.Agent.Markdown;

namespace MLS.Agent
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

        public IEnumerable<CodeLinkBlock> GetCodeLinkBlocks()
        {
            var pipeline = Project.GetMarkdownPipelineFor(Path);

            var document = Markdig.Markdown.Parse(
                ReadAllText(),
                pipeline);

            foreach (var codeLinkBlock in document.OfType<CodeLinkBlock>())
            {
                codeLinkBlock.MarkdownFile = Path;
                yield return codeLinkBlock;
            }
        }

        public async Task<IHtmlContent> ToHtmlContentAsync()
        {
            var pipeline = Project.GetMarkdownPipelineFor(Path);
            var html = await pipeline.ToHtmlAsync(ReadAllText());
            return new HtmlString(html);

            //var document = Markdig.Markdown.Parse(
            //    ReadAllText(),
            //    pipeline);

            //foreach (var codeLinkBlock in document.OfType<CodeLinkBlock>())
            //{
            //    await codeLinkBlock.InitializeAsync();
            //}

            //using (var writer = new StringWriter())
            //{
            //    var renderer = new HtmlRenderer(writer);
            //    pipeline.Setup(renderer);
            //    renderer.Render(document);
            //    var html = writer.ToString();
            //    return new HtmlString(html);
            //}
        }

        public string ReadAllText() =>
            Project.DirectoryAccessor.ReadAllText(Path);
    }
}