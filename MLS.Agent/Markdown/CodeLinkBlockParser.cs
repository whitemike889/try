using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlockParser : FencedBlockParserBase<CodeLinkBlock>
    {
        private readonly Configuration _config;

        public CodeLinkBlockParser(Configuration config)
        {
            OpeningCharacters = new[] { '`' };
            InfoParser = InfoStringParser;
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        protected override CodeLinkBlock CreateFencedBlock(BlockProcessor processor)
        {
            var block = new CodeLinkBlock(this);
            return block;
        }

        private bool InfoStringParser(BlockProcessor state, ref StringSlice line, IFencedBlock fenced)
        {
            // line.Text contains the entire string of the document
            // In the ParseBlock method we parse the first line of the fenced block which will be given by line.toString()
            // This is the line that will contain the filename and all the other trydotnet related config

            var slices = line.ToString().Split();
            if (slices.Length < 2) //We should have atleast two parts - the language string and the name of the file
            {
                return false;
            }

            var langString = slices[0];
            var argString = slices[1];

            if (!IsCSharp(langString))
            {
                return false;
            } 

            fenced.Info = HtmlHelper.Unescape(langString);
            var codeLinkBlock = fenced as CodeLinkBlock;

            if (TryGetCodeFromFile(argString, out var code))
            {
                codeLinkBlock.CodeLines = new StringSlice(HtmlHelper.Unescape(code));
            }
            else
            {
                codeLinkBlock.ExceptionMessage = $"Error reading the file {argString}";
            }

            return true;
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
