using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Microsoft.DotNet.Try.Markdown;
using MLS.Agent.Tools;
using MLS.Project.Extensions;
using WorkspaceServer;

namespace MLS.Agent.Markdown
{
    public class LocalCodeLinkBlockOptions : CodeLinkBlockOptions
    {
        public LocalCodeLinkBlockOptions(
            RelativeFilePath sourceFile = null,
            RelativeFilePath destinationFile = null,
            FileInfo project = null,
            string package = null,
            string region = null,
            string session = null,
            bool isProjectFileImplicit = false,
            bool editable = false,
            bool hidden = false,
            string runArgs = null,
            ParseResult parseResult = null,
            string packageVersion = null) : base(destinationFile, package, region, session, editable, hidden, runArgs, parseResult ,packageVersion)
        {
            SourceFile = sourceFile;
            Project = project;
            IsProjectImplicit = isProjectFileImplicit;
        }

        public FileInfo Project { get; }

        public RelativeFilePath SourceFile { get; }

        public bool IsProjectImplicit { get; set; }

        public AsyncLazy<IDirectoryAccessor> DirectoryAccessor { get; set; }

        public override async Task<CodeBlockContentFetchResult> TryGetExternalContent()
        {
            string content = null;

            var errors = new List<string>();

            await Validate(errors);

            if (!errors.Any())
            {
                if (SourceFile == null)
                {
                    return CodeBlockContentFetchResult.None;
                }

                content = (await DirectoryAccessor.ValueAsync()).ReadAllText(SourceFile);

                if (string.IsNullOrWhiteSpace(Region))
                {
                    return errors.Any()
                               ? CodeBlockContentFetchResult.Failed(errors)
                               : CodeBlockContentFetchResult.Succeeded(content);
                }

                var sourceText = SourceText.From(content);
                var sourceFileAbsolutePath = await GetSourceFileAbsolutePath();

                var buffers = sourceText.ExtractBuffers(sourceFileAbsolutePath)
                                        .Where(b => b.Id.RegionName == Region)
                                        .ToArray();

                if (buffers.Length == 0)
                {
                    errors.Add($"Region \"{Region}\" not found in file {sourceFileAbsolutePath}");
                }
                else if (buffers.Length > 1)
                {
                    errors.Add($"Multiple regions found: {Region}");
                }
                else
                {
                    content = buffers[0].Content;
                }
            } 

            return errors.Any()
                       ? CodeBlockContentFetchResult.Failed(errors)
                       : CodeBlockContentFetchResult.Succeeded(content);
        }

        private async Task Validate(List<string> errors)
        {
            IDirectoryAccessor accessor;
            try
            {
                accessor = await DirectoryAccessor.ValueAsync();
            }
            catch (PackageNotFoundException e)
            {
                errors.Add(e.Message);
                return;
            }

            if (SourceFile != null && !accessor.FileExists(SourceFile))
            {
                errors.Add($"File not found: {SourceFile.Value}");
            }

            if (Editable && string.IsNullOrEmpty(Package) && Project == null)
            {
                errors.Add("No project file or package specified");
            }

            if (Package != null && !IsProjectImplicit)
            {
                errors.Add("Can't specify both --project and --package");
            }

            if (Project != null)
            {
                var packageName = GetPackageNameFromProjectFile(Project);

                if (packageName == null)
                {
                    errors.Add($"No project file could be found at path {accessor.GetFullyQualifiedPath(new RelativeDirectoryPath("."))}");
                }
            }
        }

        public override async Task AddAttributes(CodeLinkBlock block)
        {
            if (Package == null && Project?.FullName != null)
            {
                block.AddAttribute("data-trydotnet-package", Project.FullName);
            }

            var fileName = await GetDestinationFileAbsolutePath();

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                block.AddAttribute(
                    "data-trydotnet-file-name",
                    fileName);
            }

            await base.AddAttributes(block);
        }

        private async Task<string> GetDestinationFileAbsolutePath()
        {
            var file = DestinationFile ?? SourceFile;
            return file == null
                       ? string.Empty
                       : (await DirectoryAccessor.ValueAsync())
                         .GetFullyQualifiedPath(file)
                         .FullName;
        }

        private static string GetPackageNameFromProjectFile(FileInfo projectFile)
        {
            return projectFile?.FullName;
        }

        private async Task<string> GetSourceFileAbsolutePath()
        {
            return (await DirectoryAccessor.ValueAsync()).GetFullyQualifiedPath(SourceFile).FullName;
        }
    }
}