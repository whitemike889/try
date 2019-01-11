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

                run.Handler = CommandHandler.Create((IConsole console) =>
                {
                    var registry = PackageRegistry.CreateForHostedMode();

                    foreach (var workspace in registry)
                    {
                        console.Out.WriteLine(workspace.PackageName);
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
                var package = new Command("package", "create a package");
                package.Argument = new Argument<DirectoryInfo>();
                package.Argument.Name = typeof(PackageCommand).GetMethod(nameof(PackageCommand.Do)).GetParameters()
                                         .First(p => p.ParameterType == typeof(DirectoryInfo))
                                         .Name;

                package.Handler = CommandHandler.Create<DirectoryInfo>(PackageCommand.Do);
                return package;
            }

            Command Install()
            {
                var install = new Command("install", "install a package");
                install.Argument = new Argument<string>();
                install.Argument.Name = typeof(InstallCommand).GetMethod(nameof(InstallCommand.Do)).GetParameters()
                                         .First(p => p.ParameterType == typeof(string))
                                         .Name;

                var option = new Option("--package-source", argument: new Argument<string>());

                install.AddOption(option);

                install.Handler = CommandHandler.Create<string, string>(InstallCommand.Do);
                return install;
            }
        }
    }
}