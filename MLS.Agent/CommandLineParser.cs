using System;
using Microsoft.DotNet.Cli.CommandLine;
using static Microsoft.DotNet.Cli.CommandLine.Accept;

namespace MLS.Agent
{
    public class CommandLineOptions
    {
        public CommandLineOptions(bool wasSuccess, bool helpRequested, string helpText, bool isProduction, string key)
        {
            WasSuccess = wasSuccess;
            IsProduction = isProduction;
            HelpRequested = helpRequested;
            HelpText = helpText;
            Key = key;
        }

        public bool WasSuccess { get; }
        public bool IsProduction { get; }
        public bool HelpRequested { get; }
        public string HelpText { get; }
        public string Key { get; }

        public static CommandLineOptions Parse(string[] args)
        {
            var parser = new Parser(
                Create.Option("-h|--help", "Shows this help text"),
                Create.Option("--production", "Specifies if the agent is being run using production resources or not"),
                Create.Option("-k|--key", "The encryption key", ExactlyOneArgument()));

            var parseResult = parser.Parse(args);
            var wasSuccess = true;
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
                    errorText += $"{Environment.NewLine}{error.Message}";
                }
            }

            var helpRequested = parseResult.HasOption("help");

            return new CommandLineOptions(
                wasSuccess,
                helpRequested,
                helpText: helpRequested
                              ? parseResult.Command().HelpView()
                              : errorText,
                isProduction: parseResult.HasOption("production"),
                key: parseResult.HasOption("key")
                         ? parseResult["key"].Value<string>()
                         : null);
        }
    }
}
