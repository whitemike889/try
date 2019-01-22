using System.Collections.Generic;
using System.Linq;
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

        public IHtmlContent ToHtmlContent()
        {
            var pipeline = Project.GetMarkdownPipelineFor(Path);

            var html = Markdig.Markdown.ToHtml(ReadAllText(), pipeline);

            return new HtmlString(html);
        }

        public string ReadAllText() =>
            Project.DirectoryAccessor.ReadAllText(Path);
    }
}