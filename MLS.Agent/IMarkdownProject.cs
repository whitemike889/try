using System.Collections.Generic;

namespace MLS.Agent
{
    public interface IMarkdownProject
    {
        IEnumerable<MarkdownFile> GetAllMarkdownFiles();

        bool TryGetHtmlContent(string path, out string html);
    }
}