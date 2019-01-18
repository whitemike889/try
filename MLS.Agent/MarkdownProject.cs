using System;
using System.Collections.Generic;
using System.Linq;
using Markdig;
using MLS.Agent.Markdown;

namespace MLS.Agent
{
    public class MarkdownProject
    {
        private readonly IDirectoryAccessor _directoryAccessor;

        public MarkdownProject(IDirectoryAccessor directoryAccessor)
        {
            _directoryAccessor = directoryAccessor ?? throw new ArgumentNullException(nameof(directoryAccessor));
        }

        public IEnumerable<RelativeFilePath> GetAllMarkdownFiles()
        {
           return _directoryAccessor.GetAllFilesRecursively().Where(file => file.Extension == ".md");
        }

        public bool TryGetHtmlContent(RelativeFilePath path, out string html)
        {
            html = null;

            if(!_directoryAccessor.FileExists(path))
            {
                return false;
            }

            html = ConvertToHtml(path, _directoryAccessor.ReadAllText(path));
            return true;
        }

        private string ConvertToHtml(RelativeFilePath filePath, string content)
        {
            var relativeAccessor = _directoryAccessor.GetDirectoryAccessorForRelativePath(filePath.Directory);

            var pipeline = new MarkdownPipelineBuilder()
               .UseCodeLinks(relativeAccessor)
               .Build();

            return Markdig.Markdown.ToHtml(content, pipeline);
        }
    }
}