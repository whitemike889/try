using System.Collections.Generic;
using System.IO;
using System.Linq;
using Markdig.Parsers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Microsoft.CodeAnalysis.Text;
using MLS.Project.Extensions;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlockOptions
    {
        public CodeLinkBlockOptions(
            RelativeFilePath sourceFile = null, 
            FileInfo project = null, 
            string package = null, 
            string region = null, 
            string session = null,
            bool isProjectFileImplicit = false,
            IEnumerable<string> errors = null)
        {
            SourceFile = sourceFile;
            Project = project;
            Package = package;
            Region = region;
            Session = session;
            IsProjectImplicit = isProjectFileImplicit;
            Errors = errors ?? Enumerable.Empty<string>();
        }

        public CodeLinkBlockOptions WithIsProjectImplicit(bool isProjectFileImplicit)
        {
            return new CodeLinkBlockOptions(
                        SourceFile,
                        Project,
                        Package,
                        Region,
                        Session,
                        isProjectFileImplicit: isProjectFileImplicit,
                        errors: Errors);
        }

        public CodeLinkBlockOptions WithErrors(IEnumerable<string> errors)
        {
            return new CodeLinkBlockOptions(
                        SourceFile,
                        Project,
                        Package,
                        Region,
                        Session,
                        IsProjectImplicit,
                        errors: errors);
        }

        public FileInfo Project { get; }
        public string Package { get; }
        public RelativeFilePath SourceFile { get; }
        public string Region { get; }
        public string Session { get; }
        public bool IsProjectImplicit { get; internal set; }
        public IEnumerable<string> Errors { get; }
    }

    public class CodeLinkBlock : FencedCodeBlock
    {
        private readonly IDirectoryAccessor _directoryAccessor;
        private FileInfo _projectFile;
        private RelativeFilePath _sourceFile;
        private string _region;
        private string _session;
        private string _sourceCode;
        private string _package;
        private readonly List<MarkdownProjectDiagnostic> _diagnostics = new List<MarkdownProjectDiagnostic>();

        public CodeLinkBlock(
            BlockParser parser,
            IDirectoryAccessor directoryAccessor) : base(parser)
        {
            _directoryAccessor = directoryAccessor;

            AddAttribute("data-trydotnet-mode", "editor");
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

                if (!string.IsNullOrWhiteSpace(_region))
                {
                    AddAttribute("data-trydotnet-region", Region);
                }
            }
        }
        public string Session
        {
            get => _session;
            set
            {
                _session = value;

                if (!string.IsNullOrWhiteSpace(_session))
                {
                    AddAttribute("data-trydotnet-session-id", Session);
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
                    var sourceFileAbsolutePath = GetSourceFileAbsolutePath();

                    var buffers = sourceText.ExtractBuffers(sourceFileAbsolutePath)
                                            .Where(b => b.Id.RegionName == Region)
                                            .ToArray();

                    if (buffers.Length == 0)
                    {
                        AddDiagnostic($"Region \"{Region}\" not found in file {sourceFileAbsolutePath}");
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
    }
}