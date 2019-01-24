using System;
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
    public class CodeLinkBlock : FencedCodeBlock
    {
        private readonly IDirectoryAccessor _directoryAccessor;
        private CodeLinkBlockOptions _options;
        private string _sourceCode;
        private readonly List<string> _diagnostics = new List<string>();

        public CodeLinkBlock(
            BlockParser parser,
            IDirectoryAccessor directoryAccessor) : base(parser)
        {
            _directoryAccessor = directoryAccessor;

            AddAttribute("data-trydotnet-mode", "editor");
        }

        public void AddOptions(CodeLinkBlockOptions options)
        {
            _options = options;

            AddAttributeIfNotNull(options.Project, "package");
            AddAttributeIfNotNull(options.Region, "region");
            AddAttributeIfNotNull(options.Session, "session-id");
            AddAttributeIfNotNull(Package, "package");

            if (options.SourceFile != null)
            {
                AddAttribute(
                    "data-trydotnet-file-name",
                    GetSourceFileAbsolutePath());
            }
        }

        private void AddAttributeIfNotNull(object o, string name)
        {
            if (o != null)
            {
                AddAttribute($"data-trydotnet-{name}", o.ToString());
            }
        }

        public FileInfo ProjectFile
        {
            get => _options.Project;
        }

        public string Package
        {
            get
            {
                if (_options.Package != null)
                {
                    return _options.Package;
                }

                if (_options.Project != null)
                {
                    return _options.Project.FullName;
                }

                throw new InvalidOperationException("Options contains neither project nor package");
            }
        }

        public RelativeFilePath SourceFile
        {
            get => _options.SourceFile;
        }

        public string Region
        {
            get => _options.Region;
        }

        public string Session
        {
            get => _options.Session;
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

        private string GetSourceFileAbsolutePath()
        {
            return _directoryAccessor.GetFullyQualifiedPath(_options.SourceFile).FullName;
        }

        public IEnumerable<string> Diagnostics => _diagnostics;

        public RelativeFilePath MarkdownFile { get; internal set; }

        public void AddAttribute(string key, string value)
        {
            this.GetAttributes().AddProperty(key, value);
        }

        public void AddDiagnostic(string message) =>
            _diagnostics.Add(message);
    }
}