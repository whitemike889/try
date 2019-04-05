using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Linq;
using System.Text.RegularExpressions;
using Markdig;

namespace Microsoft.DotNet.Try.Markdown
{
    public class CodeFenceOptionsParser
    {
        private readonly IDefaultCodeLinkBlockOptions _defaultOptions;
        private readonly Parser _parser;
        private readonly Lazy<ModelBinder> _modelBinder;
        private string packageOptionName = "--package";
        private string packageVersionOptionName = "--package-version";

        public CodeFenceOptionsParser(
            IDefaultCodeLinkBlockOptions defaultOptions = null,
            Action<Command> configureCsharpCommand = null)
        {
            _defaultOptions = defaultOptions;
            _parser = CreateOptionsParser(configureCsharpCommand);
            _modelBinder = new Lazy<ModelBinder>(CreateModelBinder);
        }

        protected virtual ModelBinder CreateModelBinder() => new ModelBinder(typeof(CodeLinkBlockOptions));

        public virtual CodeFenceOptionsParseResult TryParseCodeFenceOptions(
            string line,
            MarkdownParserContext parserContext = null)
        {
            if (parserContext.TryGetDefaultCodeBlockAnnotations(out var defaults))
            {
                if (defaults.PackageName != null &&
                    !line.Contains(packageOptionName))
                {
                    line += $" {packageOptionName} {defaults.PackageName}";
                }

                if (defaults.PackageVersion != null &&
                    !line.Contains(packageVersionOptionName))
                {
                    line += $" {packageVersionOptionName} {defaults.PackageVersion}";
                }
            }

            var result = _parser.Parse(line);

            CodeLinkBlockOptions options = null;

            if (result.CommandResult.Name != "csharp" ||
                result.Tokens.Count == 1)
            {
                return CodeFenceOptionsParseResult.None;
            }

            if (result.Errors.Any())
            {
                return CodeFenceOptionsParseResult.Failed(new List<string>(result.Errors.Select(e => e.Message)));
            }
            else
            {
                options = (CodeLinkBlockOptions) _modelBinder.Value.CreateInstance(new BindingContext(result));

                options.Language = result.Tokens.First().Value;
                options.RunArgs = Untokenize(result);

                return CodeFenceOptionsParseResult.Succeeded(options);
            }
        }

        private static string Untokenize(ParseResult result) =>
            string.Join(" ", result.Tokens
                                   .Select(t => t.Value)
                                   .Skip(1)
                                   .Select(t => Regex.IsMatch(t, @".*\s.*")
                                                    ? $"\"{t}\""
                                                    : t));

        private Parser CreateOptionsParser(Action<Command> configureCsharpCommand = null)
        {
            var packageOption = new Option(packageOptionName,
                                           argument: new Argument<string>());

            if (_defaultOptions?.Package is string defaultPackage)
            {
                packageOption.Argument.SetDefaultValue(defaultPackage);
            }

            var packageVersionOption = new Option(packageVersionOptionName,
                                                  argument: new Argument<string>());

            if (_defaultOptions?.PackageVersion is string defaultPackageVersion)
            {
                packageVersionOption.Argument.SetDefaultValue(defaultPackageVersion);
            }

            var csharp = new Command("csharp")
                         {
                             new Option("--destination-file",
                                        argument: new Argument<RelativeFilePath>()),
                             new Option("--editable",
                                        argument: new Argument<bool>(defaultValue: true)),
                             new Option("--hidden",
                                        argument: new Argument<bool>(defaultValue: false)),
                             new Option("--region",
                                        argument: new Argument<string>()),
                             packageOption,
                             packageVersionOption,
                             new Option("--session",
                                        argument: new Argument<string>())
                         };

            configureCsharpCommand?.Invoke(csharp);

            csharp.AddAlias("CS");
            csharp.AddAlias("C#");
            csharp.AddAlias("CSHARP");
            csharp.AddAlias("cs");
            csharp.AddAlias("c#");

            return new Parser(new RootCommand { csharp });
        }
    }
}