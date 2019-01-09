using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Markdig.Renderers.Html;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MLS.Protocol.Execution;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlockParser : FencedBlockParserBase<CodeLinkBlock>
    {
        private static readonly Parser _csharpLinkParser = CreateLineParser();

        private readonly Configuration _config;

        public CodeLinkBlockParser(Configuration config)
        {
            OpeningCharacters = new[] { '`' };
            InfoParser = ParseCodeOptions;
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        protected override CodeLinkBlock CreateFencedBlock(BlockProcessor processor)
        {
            var block = new CodeLinkBlock(this);
            return block;
        }

        private static Parser CreateLineParser()
        {
            var bufferIdArg = new Argument<BufferId>(
                               result =>
                               {
                                   return ArgumentParseResult.Success(BufferId.Parse(result.Arguments.Single()));
                               })
            {
                Name = "bufferId",
                Arity = ArgumentArity.ExactlyOne
            };

            var language = new Command("csharp", argument: bufferIdArg)
                          {
                              new Option("--project",
                                         argument: new Argument<DirectoryInfo>().ExistingOnly())
                          };

            return new Parser(language);
        }

        private bool ParseCodeOptions(
            BlockProcessor state,
            ref StringSlice line,
            IFencedBlock fenced)
        {
            // line.Text contains the entire string of the document
            // In the ParseBlock method we parse the first line of the fenced block which will be given by line.toString()
            // This is the line that will contain the filename and all the other trydotnet related config

            var codeLinkBlock = fenced as CodeLinkBlock;

            if (fenced == null)
            {
                return false;
            }

            var slices = line.ToString().Split(
                new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

            var langString = slices[0];
            var argString = slices[1];

            if (!IsCSharp(langString))
            {
                return false;
            }

            var parseResult = _csharpLinkParser.Parse(argString);

            if (parseResult.Errors.Any())
            {
                codeLinkBlock.ErrorMessage =
                    string.Join("\n", parseResult.Errors.Select(e => e.ToString()));
                return true;
            }

            fenced.Info = HtmlHelper.Unescape(langString);

            if (TryGetCodeFromFile(argString, out var code))
            {
                codeLinkBlock.CodeLines = new StringSlice(HtmlHelper.Unescape(code));
            }
            else
            {
                codeLinkBlock.ErrorMessage = $"Error reading the file {argString}";
            }

            AddAttribute(codeLinkBlock, "data-trydotnet-mode", "editor");
            AddAttribute(codeLinkBlock, "data-trydotnet-project-template", "console");
            AddAttribute(codeLinkBlock, "data-trydotnet-session-id", "a");

            return true;
        }

        private void AddAttribute(CodeLinkBlock block, string key, string value)
        {
            block.GetAttributes().AddProperty(key, value);
        }

        private bool IsCSharp(string language) => Regex.Match(language, @"cs|csharp|c#", RegexOptions.IgnoreCase).Success;


        private bool TryGetCodeFromFile(string filename, out string code)
        {
            code = null;
            var filePath = HtmlHelper.Unescape(filename).Trim();

            if (IsValidFilePath(filePath))
            {
                var fullPath = GetFullyQualifiedPath(filePath);
                if (File.Exists(fullPath))
                {
                    code = File.ReadAllText(fullPath);
                    return true;
                }
            }

            return false;
        }

        private bool IsValidFilePath(string filePath)
        {
            return filePath.IndexOfAny(Path.GetInvalidPathChars()) == -1;
        }

        private string GetFullyQualifiedPath(string filePath)
        {
            return Path.IsPathRooted(filePath)
                ? filePath
                : Path.Combine(_config.RootDirectory.FullName, filePath);
        }
    }
}
