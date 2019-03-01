using Buildalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;

//adpated from https://github.com/daveaglick/Buildalyzer/blob/master/src/Buildalyzer.Workspaces/AnalyzerResultExtensions.cs

namespace WorkspaceServer.Packaging
{
    public static class AnalyzerResultExtensions
    {
        public static CSharpParseOptions GetCSharpParseOptions(this AnalyzerResult analyzerResult)
        {
            CSharpParseOptions parseOptions = new CSharpParseOptions();

            // Add any constants
            string constants = analyzerResult.GetProperty("DefineConstants");
            if (!string.IsNullOrWhiteSpace(constants))
            {
                parseOptions = parseOptions
                    .WithPreprocessorSymbols(constants.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
            }

            // Get language version
            string langVersion = analyzerResult.GetProperty("LangVersion");
            if (!string.IsNullOrWhiteSpace(langVersion)
                && LanguageVersionFacts.TryParse(langVersion, out LanguageVersion languageVersion))
            {
                parseOptions = parseOptions.WithLanguageVersion(languageVersion);
            }

            return parseOptions;
        }
    }
}
