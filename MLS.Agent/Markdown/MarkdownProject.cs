using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Markdig;
using Recipes;
using WorkspaceServer;

namespace MLS.Agent.Markdown
{
    public class MarkdownProject
    {
        private class NullDirectoryAccessor : IDirectoryAccessor
        {
            public bool FileExists(RelativeFilePath filePath)
            {
                return false;
            }

            public string ReadAllText(RelativeFilePath filePath)
            {
                return string.Empty;
            }

            public IEnumerable<RelativeFilePath> GetAllFilesRecursively()
            {
                return Enumerable.Empty<RelativeFilePath>();
            }

            public FileSystemInfo GetFullyQualifiedPath(RelativePath path)
            {
                return null;
            }

            public IDirectoryAccessor GetDirectoryAccessorForRelativePath(RelativeDirectoryPath relativePath)
            {
                return this;
            }
        }
      
        internal IDirectoryAccessor DirectoryAccessor { get; }
        private readonly PackageRegistry _packageRegistry;

        private readonly Dictionary<RelativeFilePath, MarkdownPipeline> _markdownPipelines = new Dictionary<RelativeFilePath, MarkdownPipeline>();

        internal MarkdownProject(PackageRegistry packageRegistry) : this(new NullDirectoryAccessor(), packageRegistry)
        {

        }

        public MarkdownProject(IDirectoryAccessor directoryAccessor, PackageRegistry packageRegistry)
        {
            DirectoryAccessor = directoryAccessor ?? throw new ArgumentNullException(nameof(directoryAccessor));
            _packageRegistry = packageRegistry ?? throw new ArgumentNullException(nameof(packageRegistry));
        }

        public IEnumerable<MarkdownFile> GetAllMarkdownFiles() =>
            DirectoryAccessor.GetAllFilesRecursively()
                             .Where(file => file.Extension == ".md")
                             .Select(file => new MarkdownFile(file, this));

        public bool TryGetMarkdownFile(RelativeFilePath path, out MarkdownFile markdownFile)
        {
            if (!DirectoryAccessor.FileExists(path))
            {
                markdownFile = null;
                return false;
            }

            markdownFile = new MarkdownFile(path, this);
            return true;
        }

        internal MarkdownPipeline GetMarkdownPipelineFor(RelativeFilePath filePath)
        {
            return _markdownPipelines.GetOrAdd(filePath, key =>
            {
                var relativeAccessor = DirectoryAccessor.GetDirectoryAccessorForRelativePath(filePath.Directory);

                return new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .UseCodeLinks(relativeAccessor, _packageRegistry)
                    .Build();
            });
        }
    }
}