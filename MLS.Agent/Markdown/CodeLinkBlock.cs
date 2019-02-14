using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Markdig.Parsers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Microsoft.CodeAnalysis.Text;
using MLS.Agent.Tools;
using MLS.Project.Extensions;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlock : FencedCodeBlock
    {
        private AsyncLazy<IDirectoryAccessor> _directoryAccessor;
        private CodeLinkBlockOptions _options;
        private string _sourceCode;
        private readonly List<string> _diagnostics = new List<string>();

        public CodeLinkBlock(
            BlockParser parser) : base(parser)
        {
        }

        public void AddOptions(CodeLinkBlockOptions options, Func<Task<IDirectoryAccessor>> directoryAccessor)
        {
            _options = options;
            _directoryAccessor = new AsyncLazy<IDirectoryAccessor>(directoryAccessor);
        }

        public async Task InitializeAsync()
        {
            if (_options == null)
            {
                throw new InvalidOperationException("Attempted to initialize block before adding options");
            }

            if (await ValidateOptions(_options))
            {
                await SetSourceCode();
                await AddAttributes(_options);
            }
        }

        private async Task SetSourceCode()
        {
            if (SourceFile != null)
            {
                _sourceCode = (await _directoryAccessor.ValueAsync()).ReadAllText(SourceFile);

                if (!string.IsNullOrWhiteSpace(Region))
                {
                    var sourceText = SourceText.From(_sourceCode);
                    var sourceFileAbsolutePath = await GetSourceFileAbsolutePath();

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

                if (_sourceCode != null)
                {
                    Lines = new Markdig.Helpers.StringLineGroup(_sourceCode);
                }
            }

            _sourceCode = _sourceCode ?? "";
        }

        private async Task AddAttributes(CodeLinkBlockOptions options)
        {
            AddAttribute("data-trydotnet-mode", "editor");
            AddAttributeIfNotNull("package", options.Project);
            AddAttributeIfNotNull("region", options.Region);
            AddAttributeIfNotNull("session-id", options.Session);
            AddAttributeIfNotNull("package", Package);

            if (options.SourceFile != null)
            {
                AddAttribute(
                    "data-trydotnet-file-name",
                    await GetSourceFileAbsolutePath());
            }
        }

        private async Task<bool> ValidateOptions(CodeLinkBlockOptions options)
        {
            bool succeeded = true;

            IDirectoryAccessor accessor = null;
            try
            {
                accessor = await _directoryAccessor.ValueAsync();
            }
            catch (ArgumentException e)
            {
                AddDiagnostic(e.Message);
                return false;
            }

            if (options.SourceFile != null && !accessor.FileExists(options.SourceFile))
            {
                AddDiagnostic($"File not found: {options.SourceFile.Value}");
            }

            if (string.IsNullOrEmpty(options.Package) && options.Project == null)
            {
                AddDiagnostic("No project file or package specified");
            }

            if (options.Package != null && !options.IsProjectImplicit)
            {
                AddDiagnostic("Can't specify both --project and --package");
                succeeded = false;
            }

            foreach (var error in options.Errors)
            {
                AddDiagnostic(error);
                succeeded = false;
            }

            if (options.Project != null)
            {
                var packageName = GetPackageNameFromProjectFile(options.Project);

                if (packageName == null)
                {
                    AddDiagnostic(
                        $"No project file could be found at path {accessor.GetFullyQualifiedPath(new RelativeDirectoryPath("."))}");
                    succeeded = false;
                }
            }

            return succeeded;
        }

        private static string GetPackageNameFromProjectFile(FileInfo projectFile)
        {
            return projectFile?.FullName;
        }

        private void AddAttributeIfNotNull(string name, object o)
        {
            if (o != null)
            {
                AddAttribute($"data-trydotnet-{name}", o.ToString());
            }
        }

        public FileInfo ProjectFile => _options.Project;

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

                return string.Empty;
            }
        }

        public RelativeFilePath SourceFile => _options.SourceFile;

        public string Region => _options.Region;

        public string RunArgs => _options.RunArgs;

        public string Session => _options.Session;

        public string SourceCode
        {
            get
            {
                if (_sourceCode == null)
                {
                    throw new InvalidOperationException($"Attempted to retrieve {nameof(SourceCode)} from uninitialized {nameof(CodeLinkBlock)}");
                }

                return _sourceCode;
            }
        }

        private async Task<string> GetSourceFileAbsolutePath()
        {
            return (await _directoryAccessor.ValueAsync()).GetFullyQualifiedPath(_options.SourceFile).FullName;
        }

        public IEnumerable<string> Diagnostics => _diagnostics;

        public void AddAttribute(string key, string value)
        {
            this.GetAttributes().AddProperty(key, value);
        }

        public void AddDiagnostic(string message) =>
            _diagnostics.Add(message);
    }
}