using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using Markdig.Parsers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Microsoft.CodeAnalysis.Text;
using MLS.Project.Extensions;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlock : FencedCodeBlock
    {
        private readonly IDirectoryAccessor _directoryAccessor;
        private FileInfo _projectFile;
        private RelativeFilePath _sourceFile;
        private string _region;
        private string _sourceCode;
        private string _package;
        private readonly List<MarkdownProjectDiagnostic> _diagnostics = new List<MarkdownProjectDiagnostic>();

        public CodeLinkBlock(
            BlockParser parser,
            IDirectoryAccessor directoryAccessor) : base(parser)
        {
            _directoryAccessor = directoryAccessor;

            AddAttribute("data-trydotnet-mode", "editor");
            AddAttribute("data-trydotnet-session-id", "a");
        }

        public FileInfo ProjectFile
        {
            get => _projectFile;
            set
            {
                _projectFile = value;

                if (value != null)
                {
                    AddAttribute("data-trydotnet-package", value.FullName);
                }
            }
        }

        public string Package
        {
            get => _package;
            set
            {
                _package = value;

                if (value != null)
                {
                    AddAttribute("data-trydotnet-package", value);
                }
            }
        }

        public RelativeFilePath SourceFile
        {
            get => _sourceFile;
            set
            {
                _sourceFile = value;

                if (value != null)
                {
                    AddAttribute(
                        "data-trydotnet-file-name",
                        GetSourceFileAbsolutePath());
                }
            }
        }

        private string GetSourceFileAbsolutePath()
        {
            return _directoryAccessor.GetFullyQualifiedPath(_sourceFile).FullName;
        }

        public string Region
        {
            get => _region;
            set
            {
                _region = value;

                if (!string.IsNullOrWhiteSpace(Region))
                {
                    AddAttribute("data-trydotnet-region", Region);
                }
            }
        }

        public string SourceCode
        {
            get
            {
                if (_sourceCode != null)
                {
                    return _sourceCode;
                }

                if (SourceFile == null)
                {
                    return "";
                }

                _sourceCode = _directoryAccessor.ReadAllText(SourceFile);

                if (!string.IsNullOrWhiteSpace(Region))
                {
                    var sourceText = SourceText.From(_sourceCode);
                    var buffers = sourceText.ExtractBuffers(GetSourceFileAbsolutePath())
                                            .Where(b => b.Id.RegionName == Region)
                                            .ToArray();

                    if (buffers.Length == 0)
                    {
                        AddDiagnostic($"Region not found: {Region}");
                    }
                    else if (buffers.Length > 1)
                    {
                        AddDiagnostic($"Multiple regions found: {Region}");
                    }
                    else
                    {
                        _sourceCode = buffers[0].Content;
                    }
                }

                return _sourceCode;
            }
        }

        public IEnumerable<MarkdownProjectDiagnostic> Diagnostics => _diagnostics;

        public RelativeFilePath MarkdownFile { get; internal set; }

        public void AddAttribute(string key, string value)
        {
            this.GetAttributes().AddProperty(key, value);
        }

        public void AddDiagnostic(string message) =>
            _diagnostics.Add(new MarkdownProjectDiagnostic(message, this));

        public static Parser CreateOptionsParser(IDirectoryAccessor directoryAccessor)
        {
            var sourceFileArg = new Argument<RelativeFilePath>(
                                    result =>
                                    {
                                        var filename = result.Arguments.SingleOrDefault();

                                        if (filename == null)
                                        {
                                            return ArgumentParseResult.Success<string>(null);
                                        }

                                        if (RelativeFilePath.TryParse(filename, out var relativeFilePath))
                                        {
                                            if (directoryAccessor.FileExists(relativeFilePath))
                                            {
                                                return ArgumentParseResult.Success(relativeFilePath);
                                            }

                                            return ArgumentParseResult.Failure($"File not found: {relativeFilePath.Value}");
                                        }

                                        return ArgumentParseResult.Failure($"Error parsing the filename: {filename}");
                                    })
                                {
                                    Name = "SourceFile",
                                    Arity = ArgumentArity.ZeroOrOne
                                };

            var projectArg = new Argument<FileInfo>(result =>
                             {
                                 var projectPath = new RelativeFilePath(result.Arguments.Single());

                                 if (directoryAccessor.FileExists(projectPath))
                                 {
                                     return ArgumentParseResult.Success(directoryAccessor.GetFullyQualifiedPath(projectPath));
                                 }

                                 return ArgumentParseResult.Failure($"Project not found: {projectPath.Value}");
                             })
                             {
                                 Name = "project",
                                 Arity = ArgumentArity.ExactlyOne
                             };

            projectArg.SetDefaultValue(() =>
            {
                var projectFiles = directoryAccessor.GetAllFilesRecursively()
                                                    .Where(file => file.Extension == ".csproj")
                                                    .ToArray();

                if (projectFiles.Length == 1)
                {
                    return directoryAccessor.GetFullyQualifiedPath(projectFiles.Single());
                }

                return null;
            });

            var regionArgument = new Argument<string>();
            var packageArgument = new Argument<string>();

            var csharp = new Command("csharp", argument: sourceFileArg)
                         {
                             new Option("--project", argument: projectArg),
                             new Option("--region", argument: regionArgument),
                             new Option("--package", argument: packageArgument)
                         };

            csharp.AddAlias("CS");
            csharp.AddAlias("C#");
            csharp.AddAlias("CSHARP");
            csharp.AddAlias("cs");
            csharp.AddAlias("c#");

            return new Parser(new RootCommand { csharp });
        }
    }
}