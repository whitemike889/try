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
            var bufferIdArg = new Argument<BufferId>(
                              result =>
                              {
                                   BufferId bufferId = BufferId.Parse(result.Arguments.Single());
                                   if (_directoryAccessor.FileExists(bufferId.FileName))
                                   {
                                       return ArgumentParseResult.Success(bufferId);
                                   }

                                   return ArgumentParseResult.Failure($"File not found: {bufferId.FileName}");
                              })
                              {
                                    Name = "bufferId",
                                    Arity = ArgumentArity.ExactlyOne
                              };

            var language = new Command("csharp", argument: bufferIdArg)
                          {
                              new Option("--project", argument: new Argument<DirectoryInfo>().ExistingOnly())
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
            }
            else
            {
                fenced.Info = langString;
                var bufferId = parseResult.CommandResult.GetValueOrDefault<BufferId>();
                codeLinkBlock.CodeLines = new StringSlice(HtmlHelper.Unescape(_directoryAccessor.ReadAllText(bufferId.FileName)));
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
    }
}
