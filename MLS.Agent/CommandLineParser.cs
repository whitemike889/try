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
        public delegate Task Pack(DirectoryInfo packTarget, IConsole console);
        public delegate Task Install(string packageName, string packageSource, IConsole console);

        public static Parser Create(
            StartServer start,
            TryGitHub tryGithub,
            Pack pack,
            Install install)
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
            rootCommand.AddCommand(ListPackages());
            rootCommand.AddCommand(GitHub());
            rootCommand.AddCommand(Package());
            rootCommand.AddCommand(Install());

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

            Command ListPackages()
            {
                var run = new Command("list-packages", "Lists the installed Try .NET packages");

                run.Handler = CommandHandler.Create(async (IConsole console) =>
                {
                    var registry = PackageRegistry.CreateForHostedMode();

                    foreach (var package in registry)
                    {
                        console.Out.WriteLine((await package).PackageName);
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

                var github = new Command("github", "Try a GitHub repo", argument: argument);

                github.Handler = CommandHandler.Create<string, IConsole>((repo, console) => tryGithub(repo, console));

                return github;
            }

            Command Package()
            {
                var packCommand = new Command("pack", "create a package");
                packCommand.Argument = new Argument<DirectoryInfo>();
                packCommand.Argument.Name = typeof(PackageCommand).GetMethod(nameof(PackageCommand.Do)).GetParameters()
                                         .First(p => p.ParameterType == typeof(DirectoryInfo))
                                         .Name;

                packCommand.Handler = CommandHandler.Create<DirectoryInfo, IConsole>(
                    (packTarget, console) => pack(packTarget, console));

                return packCommand;
            }

            Command Install()
            {
                var installCommand = new Command("install", "install a package");
                installCommand.Argument = new Argument<string>();
                installCommand.Argument.Name = typeof(InstallCommand).GetMethod(nameof(InstallCommand.Do)).GetParameters()
                                         .First(p => p.ParameterType == typeof(string))
                                         .Name;

                var option = new Option("--add-source", argument: new Argument<string>());

                installCommand.AddOption(option);

                installCommand.Handler = CommandHandler.Create<string, string, IConsole>((packageName, packageSource, console) => install(packageName, packageSource, console));
                return installCommand;
            }
        }
    }
}