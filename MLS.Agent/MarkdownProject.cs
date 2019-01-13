using System;
using System.Collections.Generic;
using System.Linq;
using Markdig;
using MLS.Agent.Markdown;

namespace MLS.Agent
{
    public class MarkdownProject : IMarkdownProject
    {
        private readonly IDirectoryAccessor _directoryAccessor;

        public MarkdownProject(IDirectoryAccessor directoryAccessor)
        {
            _directoryAccessor = directoryAccessor ?? throw new ArgumentNullException(nameof(directoryAccessor));
        }

        public IEnumerable<MarkdownFile> GetAllMarkdownFiles()
        {
            var files = _directoryAccessor.GetAllFilesRecursively().Where(file => file.Extension == ".md");
            return files.Select(file => new MarkdownFile(file)).ToArray();
        }

        public bool TryGetHtmlContent(string path, out string html)
        {
            html = null;

            if(!_directoryAccessor.FileExists(path))
            {
                return false;
            }

            html = ConvertToHtml(path, _directoryAccessor.ReadAllText(path));
            return true;
        }

        private string ConvertToHtml(string path, string content)
        {
            var pipeline = new MarkdownPipelineBuilder()
               .UseCodeLinks(_directoryAccessor)
               .Build();

            return Markdig.Markdown.ToHtml(content, pipeline);
        }
    }
}