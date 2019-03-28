using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig;
using Microsoft.AspNetCore.Html;
using MLS.Protocol.Execution;

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

        public async Task<IEnumerable<CodeLinkBlock>> GetSourceCodeLinkBlocks()
        {
            var blocks = (await GetCodeLinkBlocks()).Where(b => !b.IsInclude);
            return blocks;
        }

        public async Task<IEnumerable<CodeLinkBlock>> GetIncludeCodeLinkBlocks()
        {
            var blocks = (await GetCodeLinkBlocks()).Where(b => b.IsInclude);
            return blocks;
        }

        public async Task<IHtmlContent> ToHtmlContentAsync()
        {
            var pipeline = Project.GetMarkdownPipelineFor(Path);
            var html = await pipeline.RenderHtmlAsync(ReadAllText());
            return new HtmlString(html);
        }

        public string ReadAllText() =>
            Project.DirectoryAccessor.ReadAllText(Path);

        public async Task<Dictionary<string, Workspace.Buffer[]>> GetBuffersToInclude(IDirectoryAccessor directoryAccessor)
        {
            var includes = new Dictionary<string, Workspace.Buffer[]>(StringComparer.InvariantCultureIgnoreCase);

            var includeFileBuffersBySession = new Dictionary<string, Dictionary<BufferId, StringBuilder>>(StringComparer.InvariantCultureIgnoreCase);

            var blocks = await GetIncludeCodeLinkBlocks();

            foreach (var block in blocks)
            {
                if (string.IsNullOrWhiteSpace(block.Region))
                {
                    continue;
                }
                var sessionId = string.IsNullOrWhiteSpace(block.Session) ? "global" : block.Session;
                var filePath = block.DestinationFile ?? new RelativeFilePath($"./generated_include_file_{sessionId}.cs");
                var bufferId =new BufferId(directoryAccessor.GetFullyQualifiedPath(filePath).FullName, block.Region);
                if (!includeFileBuffersBySession.TryGetValue(sessionId, out var sessionFileBuffers))
                {
                    sessionFileBuffers = new Dictionary<BufferId, StringBuilder>();
                    includeFileBuffersBySession[sessionId] = sessionFileBuffers;
                }

                if (!sessionFileBuffers.TryGetValue(bufferId, out var fileBuffer))
                {
                    fileBuffer = new StringBuilder();
                    sessionFileBuffers[bufferId] = fileBuffer;
                }

                fileBuffer.AppendLine(block.SourceCode);
            }

            foreach (var includeFileBuffers in includeFileBuffersBySession)
            {
                includes[includeFileBuffers.Key] = includeFileBuffers.Value.Select((fileBuffer) => new Workspace.Buffer(fileBuffer.Key, fileBuffer.Value.ToString())).ToArray();
            }

            return includes;
        }

        public async Task<Dictionary<string, Workspace.File[]>> GetFilesToInclude(IDirectoryAccessor directoryAccessor)
        {
            var includes = new Dictionary<string, Workspace.File[]>(StringComparer.InvariantCultureIgnoreCase);

            var includeFileBuffersBySession = new Dictionary<string, Dictionary<string, StringBuilder>>(StringComparer.InvariantCultureIgnoreCase);

            var blocks = await GetIncludeCodeLinkBlocks();

            foreach (var block in blocks)
            {
                if (!string.IsNullOrWhiteSpace(block.Region))
                {
                    continue;
                }
                var sessionId = string.IsNullOrWhiteSpace(block.Session) ? "global" : block.Session;
                var filePath = block.DestinationFile ?? new RelativeFilePath($"./generated_include_file_{sessionId}.cs");
                var absolutePath = directoryAccessor.GetFullyQualifiedPath(filePath).FullName;
                if (!includeFileBuffersBySession.TryGetValue(sessionId, out var sessionFileBuffers))
                {
                    sessionFileBuffers = new Dictionary<string, StringBuilder>(StringComparer.InvariantCultureIgnoreCase);
                    includeFileBuffersBySession[sessionId] = sessionFileBuffers;
                }

                if (!sessionFileBuffers.TryGetValue(absolutePath, out var fileBuffer))
                {
                    fileBuffer = new StringBuilder();
                    sessionFileBuffers[absolutePath] = fileBuffer;
                }

                fileBuffer.AppendLine(block.SourceCode);
            }

            foreach (var includeFileBuffers in includeFileBuffersBySession)
            {
                includes[includeFileBuffers.Key] = includeFileBuffers.Value.Select((fileBuffer) => new Workspace.File(fileBuffer.Key, fileBuffer.Value.ToString())).ToArray();
            }

            return includes;
        }
    }
}