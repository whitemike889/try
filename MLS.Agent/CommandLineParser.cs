using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.DotNet.Cli;
using System;
using static Microsoft.DotNet.Cli.CommandLine.DefaultHelpViewText;
using static Microsoft.DotNet.Cli.CommandLine.Accept;
using static Microsoft.DotNet.Cli.CommandLine.HelpViewExtensions;

namespace MLS.Agent
{
    public class CommandLineOptions
    {
        public readonly bool wasSuccess;
        public readonly bool production;
        public readonly bool helpRequested;
        public readonly string helpText;
        public readonly string key;
        public CommandLineOptions(bool wasSuccess, bool helpRequested, string helpText, bool production, string key)
        {
            this.wasSuccess = wasSuccess;
            this.production = production;
            this.helpRequested = helpRequested;
            this.helpText = helpText;
            this.key = key;
        }

        public static CommandLineOptions Parse(string[] args)
        {
            var parser = new Microsoft.DotNet.Cli.CommandLine.Parser(
                Create.Option("-h|--help", "Shows this help text"),
                Create.Option("--production", "Specifies if the agent is being run using production resources or not"),
                Create.Option("-k|--key", "The encryption key", ExactlyOneArgument()));

            var parseResult = parser.Parse(args);
            bool wasSuccess = true;
            string errorText = null;

            foreach (var error in parseResult.Errors)
            {
                wasSuccess = false;
                if (errorText is null)
                {
                    errorText = error.Message;
                }
                else
                {
                    errorText += $"{System.Environment.NewLine}{error.Message}";
                }
            }

            var helpRequested = parseResult.HasOption("help");

            return new CommandLineOptions(
                wasSuccess,
                helpRequested,
                helpText: helpRequested ? parseResult.Command().HelpView() : errorText,
                production: parseResult.HasOption("production"),
                key: parseResult.HasOption("key") ? parseResult["key"].Value<string>() : null);
        }
    }
}
