using System.Collections.Generic;

namespace MLS.Agent
{
    public interface IMarkdownProject
    {
        IEnumerable<MarkdownFile> GetAllFiles();
        bool TryGetHtmlContent(string path, out string html);
    }
}