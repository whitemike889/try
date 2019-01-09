using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WorkspaceServer;

namespace MLS.Agent
{
    public static class CommandLineParser
    {
        private static readonly TypeBinder _typeBinder = new TypeBinder(typeof(StartupOptions));

        public delegate void StartServer(StartupOptions options, InvocationContext context);
        public delegate Task TryGitHub(string repo, IConsole console);

        public static Parser Create(
            StartServer start,
            TryGitHub tryGithub)
        {
            var startHandler = CommandHandler.Create<InvocationContext>(context =>
            {
                var options = (StartupOptions) _typeBinder.CreateInstance(context);

                start(options, context);
            });

            var rootCommand = StartInTryMode();
            rootCommand.Handler = startHandler;

            var startInHostedMode = StartInHostedMode();
            startInHostedMode.Handler = startHandler;

            rootCommand.AddCommand(startInHostedMode);
            rootCommand.AddCommand(ListWorkspaces());
            rootCommand.AddCommand(GitHub());

            return new CommandLineBuilder(rootCommand)
                   .UseDefaults()
                   .Build();

            RootCommand StartInTryMode()
            {
                var command = new RootCommand
                              {
                                  Description = "Try out a .NET project with interactive documentation in your browser"
                              };
        
                command.AddOption(new Option(
                                      "--root-directory",
                                      "Specify the path to the root directory",
                                      new Argument<DirectoryInfo>(new DirectoryInfo(Directory.GetCurrentDirectory())).ExistingOnly()));

                return command;
            }

            Command StartInHostedMode()
            {
                var command = new Command("hosted")
                              {
                                  Description = "Starts the Try .NET agent",
                                  IsHidden = true
                              };

                command.AddOption(new Option(
                                      "--id",
                                      "A unique id for the agent instance (e.g. its development environment id).",
                                      new Argument<string>(defaultValue: () => Environment.MachineName)));
                command.AddOption(new Option(
                                      "--production",
                                      "Specifies whether the agent is being run using production resources",
                                      new Argument<bool>()));
                command.AddOption(new Option(
                                      "--language-service",
                                      "Specifies whether the agent is being run in language service-only mode",
                                      new Argument<bool>()));
                command.AddOption(new Option(
                                      new[] { "-k", "--key" },
                                      "The encryption key",
                                      new Argument<string>()));
                command.AddOption(new Option(
                                      new[] { "--ai-key", "--application-insights-key" },
                                      "Application Insights key.",
                                      new Argument<string>()));
                command.AddOption(new Option(
                                      "--region-id",
                                      "A unique id for the agent region",
                                      new Argument<string>()));
                command.AddOption(new Option(
                                      "--log-to-file",
                                      "Writes a log file",
                                      new Argument<bool>()));

                return command;
            }

            Command ListWorkspaces()
            {
                var run = new Command("list-packages", "Lists the available Try .NET packages");

                run.Handler = CommandHandler.Create((IConsole console) =>
                {
                    var registry = WorkspaceRegistry.CreateForHostedMode();

                    foreach (var workspace in registry)
                    {
                        console.Out.WriteLine(workspace.WorkspaceName);
                    }
                });

                return run;
            }

            Command GitHub()
            {
                var argument = new Argument<string>();

                // System.CommandLine parameter binding does lookup by name,
                // so name the argument after the github command's string param
                argument.Name = tryGithub.Method.GetParameters()
                                         .First(p => p.ParameterType == typeof(string))
                                         .Name;

                var run = new Command("github", "Try a GitHub repo", argument: argument);

                run.Handler = CommandHandler.Create<string, IConsole>((repo, console) => tryGithub(repo, console));

                return run;
            }
        }
    }
}