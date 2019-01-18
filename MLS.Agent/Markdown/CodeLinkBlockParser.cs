using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Markdig.Renderers.Html;
using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MLS.Project.Extensions;
using Microsoft.CodeAnalysis.Text;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlockParser : FencedBlockParserBase<CodeLinkBlock>
    {
        private readonly Parser _csharpLinkParser;

        private readonly IDirectoryAccessor _directoryAccessor;

        public CodeLinkBlockParser(IDirectoryAccessor directoryAccessor)
        {
            OpeningCharacters = new[] { '`' };
            InfoParser = ParseCodeOptions;
            _directoryAccessor = directoryAccessor ?? throw new ArgumentNullException(nameof(directoryAccessor));
            _csharpLinkParser = CreateLineParser();
        }

        protected override CodeLinkBlock CreateFencedBlock(BlockProcessor processor)
        {
            var block = new CodeLinkBlock(this);
            return block;
        }

        private Parser CreateLineParser()
        {
            var sourceFile = new Argument<RelativeFilePath>(
                              result =>
                              {
                                  var filename = result.Arguments.Single();
                                  if (RelativeFilePath.TryParse(filename, out var relativeFilePath))
                                  {
                                      if (_directoryAccessor.FileExists(relativeFilePath))
                                      {
                                          return ArgumentParseResult.Success(relativeFilePath);
                                      }

                                      return ArgumentParseResult.Failure($"File not found: {relativeFilePath.Value}");
                                  }

                                  return ArgumentParseResult.Failure($"Error parsing the filename: {filename}");
                              })
            {
                Name = "SourceFile",
                Arity = ArgumentArity.ExactlyOne
            };

            var projectArgument = new Argument<FileInfo>(result =>
            {
                var projectPath = new RelativeFilePath(result.Arguments.Single());
                if (_directoryAccessor.FileExists(projectPath))
                {
                    return ArgumentParseResult.Success(_directoryAccessor.GetFullyQualifiedPath(projectPath));
                }

                return ArgumentParseResult.Failure($"Project not found: {projectPath.Value}");
            })
            {
                Name = "project",
                Arity = ArgumentArity.ExactlyOne
            };

            projectArgument.SetDefaultValue(() =>
            {
                var projectFiles = _directoryAccessor.GetAllFilesRecursively().Where(file => file.Extension == ".csproj");
                if (projectFiles.Count() == 1)
                {
                    return _directoryAccessor.GetFullyQualifiedPath(projectFiles.Single());
                }

                return null;
            });

            var regionArgument = new Argument<string>();

            var csharp = new Command("csharp", argument: sourceFile)
                          {
                              new Option("--project", argument: projectArgument),
                              new Option("--region", argument: regionArgument)
                          };

            csharp.AddAlias("CS");
            csharp.AddAlias("C#");
            csharp.AddAlias("CSHARP");
            csharp.AddAlias("cs");
            csharp.AddAlias("c#");

            return new Parser(new RootCommand { csharp });
        }

        private bool ParseCodeOptions(
            BlockProcessor state,
            ref StringSlice line,
            IFencedBlock fenced)
        {
            var codeLinkBlock = fenced as CodeLinkBlock;

            if (fenced == null)
            {
                return false;
            }

            var parseResult = _csharpLinkParser.Parse(line.ToString());
            
            if (parseResult.Errors.Any())
            {
                if (parseResult.CommandResult.Name != "csharp")
                {
                    return false;
                }

                codeLinkBlock.ErrorMessage.AddRange(parseResult.Errors.Select(e => e.ToString()));

                return true;
            }

            var projectFile = parseResult.ValueForOption<FileInfo>("project");
            var project = GetPackageNameFromProjectFile(projectFile);
            if (project == null)
            {
                codeLinkBlock.ErrorMessage.Add($"No project file could be found at path {_directoryAccessor.GetFullyQualifiedPath(new RelativeDirectoryPath("."))}");
                return true;
            }

            var region = parseResult.ValueForOption<string>("region");
            var sourceFile = parseResult.CommandResult.GetValueOrDefault<RelativeFilePath>();
            var absoluteSourceFilePath = _directoryAccessor.GetFullyQualifiedPath(sourceFile).FullName;
            var sourceCode = GetSourceCodeToEmbed(codeLinkBlock, region, sourceFile, absoluteSourceFilePath);

            SetAttributes(codeLinkBlock, project, region, absoluteSourceFilePath, sourceCode);

            return true;
        }

        private static string GetPackageNameFromProjectFile(FileInfo projectFile)
        {
            return projectFile?.FullName;
        }

        private void SetAttributes(
            CodeLinkBlock codeLinkBlock,
            string trydotnetPackage,
            string region,
            string absoluteSourceFilePath,
            string sourceCode)
        {
            codeLinkBlock.AddAttribute("data-trydotnet-mode", "editor");
            codeLinkBlock.AddAttribute("data-trydotnet-package", trydotnetPackage);
            codeLinkBlock.AddAttribute("data-trydotnet-file-name", absoluteSourceFilePath);
            codeLinkBlock.AddAttribute("data-trydotnet-session-id", "a");
            codeLinkBlock.CodeLines = new StringSlice(sourceCode);

            if (!string.IsNullOrWhiteSpace(region))
            {
                codeLinkBlock.AddAttribute("data-trydotnet-region", region);
            }
        }

        private string GetSourceCodeToEmbed(CodeLinkBlock codeLinkBlock, string region, RelativeFilePath sourceFile, string absoluteSourceFilePath)
        {
            var sourceCode = _directoryAccessor.ReadAllText(sourceFile);

            if (!string.IsNullOrWhiteSpace(region))
            {
                var sourceText = SourceText.From(sourceCode);
                var buffers = sourceText.ExtractBuffers(absoluteSourceFilePath)
                    .Where(b => b.Id.RegionName == region).ToArray();

                if (buffers.Length == 0)
                {
                    codeLinkBlock.ErrorMessage.Add($"Region not found: {region}");
                }
                else if (buffers.Length > 1)
                {
                    codeLinkBlock.ErrorMessage.Add($"Multiple regions found: {region}");
                }
                else
                {
                    sourceCode = buffers[0].Content;
                }
            }

            return sourceCode;
        }

        private bool IsCSharp(string language) => Regex.Match(language, @"cs|csharp|c#", RegexOptions.IgnoreCase).Success;
    }
}
