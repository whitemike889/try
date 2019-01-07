using Markdig;
using MLS.Agent.Markdown;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MLS.Agent
{
    public class MarkdownProject : IMarkdownProject
    {
        private StartupOptions _startupOptions;

        public MarkdownProject(StartupOptions startupOptions)
        {
            _startupOptions = startupOptions ?? throw new ArgumentNullException(nameof(startupOptions)); ;
        }

        public IEnumerable<MarkdownFile> GetAllFiles()
        {
            var files = Directory.GetFiles(_startupOptions.RootDirectory.FullName, "*.md", SearchOption.AllDirectories);
            return files.Select(file => new MarkdownFile(new FileInfo(file))).ToArray();
        }

        public bool TryGetHtmlContent(string path, out string html)
        {
            html = ""; //TODO: ask what is the correct value here
            var files = Directory.GetFiles(_startupOptions.RootDirectory.FullName, path);
            if (files.Length == 0)
            {
                return false;
            }

            var markdownfile = new MarkdownFile(new FileInfo(files.First()));
            if (markdownfile.TryGetContent(out var content))
            {
                html = ConvertToHtml(content);
                return true;
            }

            return false;
        }

        private string ConvertToHtml(string content)
        {
            var pipeline = new MarkdownPipelineBuilder()
               .UseCodeLinks(new Configuration(_startupOptions.RootDirectory))
               .Build();

            return Markdig.Markdown.ToHtml(content, pipeline);
        }
    }
}
