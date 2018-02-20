using System;
using Microsoft.DotNet.Cli.CommandLine;
using static Microsoft.DotNet.Cli.CommandLine.Accept;

namespace MLS.Agent
{
    public class CommandLineOptions
    {
        public CommandLineOptions(
            bool wasSuccess, 
            bool helpRequested, 
            string helpText, 
            bool isProduction, 
            string key, 
            string[] loadWorkspaces,
            bool logToFile = false)
        {
            LoadWorkspaces = loadWorkspaces;
            LogToFile = logToFile;
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
        public string[] LoadWorkspaces { get; }
        public bool LogToFile { get; }

        public static CommandLineOptions Parse(string[] args)
        {
            var parser = new Parser(
                Create.Option("-h|--help", "Shows this help text"),
                Create.Option("--production", "Specifies if the agent is being run using production resources or not"),
                Create.Option("-k|--key", "The encryption key", ExactlyOneArgument()),
                Create.Option("--load-workspace", "Starts OmniSharp in the specified workspace folder.", OneOrMoreArguments()),
                Create.Option("--log-to-file", "Writes a log file", NoArguments()));

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
                logToFile: parseResult.HasOption("log-to-file"),
                key: parseResult.HasOption("key")
                         ? parseResult["key"].Value<string>()
                         : null,
                loadWorkspaces: parseResult.HasOption("load-workspace")
                                       ? parseResult.AppliedOptions["load-workspace"].Value<string[]>()
                                       : new string[] { });
        }
    }
}
