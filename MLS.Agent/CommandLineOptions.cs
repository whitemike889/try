using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;

namespace MLS.Agent
{
    public class CommandLineOptions
    {
        private static readonly TypeBinder _typeBinder = new TypeBinder(typeof(CommandLineOptions));

        public CommandLineOptions(
            bool production,
            bool languageService,
            string key = null,
            string applicationInsightsKey = null,
            bool logToFile = false,
            string id = null,
            string regionId = null)
        {
            LogToFile = logToFile;
            Id = id;
            Production = production;
            LanguageService = languageService;
            Key = key;
            ApplicationInsightsKey = applicationInsightsKey;
            RegionId = regionId;
        }

        public string Id { get; set; }
        public string RegionId { get; set; }
        public bool Production { get; set; }
        public bool LanguageService { get; set; }
        public string Key { get; set; }
        public string ApplicationInsightsKey { get; set; }
        public bool LogToFile { get; set; }

        public static Parser CreateParser(Action<CommandLineOptions, InvocationContext> invoke)
        {
            var rootCommand = new RootCommand();

            rootCommand.AddOption(new Option(
                                  "--id",
                                  "A unique id for the agent instance (e.g. its development environment id).",
                                  new Argument<string>(defaultValue: () => Environment.MachineName)));
            rootCommand.AddOption(new Option(
                                  "--production",
                                  "Specifies whether the agent is being run using production resources",
                                  new Argument<bool>()));
            rootCommand.AddOption(new Option(
                                  "--language-service",
                                  "Specifies whether the agent is being run in language service-only mode",
                                  new Argument<bool>()));
            rootCommand.AddOption(new Option(
                                  new[] { "-k", "--key" },
                                  "The encryption key",
                                  new Argument<string>()));
            rootCommand.AddOption(new Option(
                                  new[] { "--ai-key", "--application-insights-key" },
                                  "Application Insights key.",
                                  new Argument<string>()));
            rootCommand.AddOption(new Option(
                                  "--region-id",
                                  "A unique id for the agent region",
                                  new Argument<string>()));
            rootCommand.AddOption(new Option(
                                  "--log-to-file",
                                  "Writes a log file",
                                  new Argument<bool>()));

            rootCommand.Handler = CommandHandler.Create<InvocationContext>(context =>
            {
                var options = (CommandLineOptions) _typeBinder.CreateInstance(context);

                invoke(options, context);
            });

            return new CommandLineBuilder(rootCommand)
                   .UseDefaults()
                   .Build();
        }
    }
}
