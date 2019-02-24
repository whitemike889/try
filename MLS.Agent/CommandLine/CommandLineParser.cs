using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Packaging;
using CommandHandler = System.CommandLine.Invocation.CommandHandler;

namespace MLS.Agent.CommandLine
{
    public static class CommandLineParser
    {
        public delegate void StartServer(StartupOptions options, InvocationContext context);
        public delegate Task Demo(DemoOptions options, IConsole console);
        public delegate Task TryGitHub(TryGitHubOptions options, IConsole console);
        public delegate Task Pack(PackOptions options, IConsole console);
        public delegate Task Install(InstallOptions options, IConsole console);
        public delegate Task<int> Verify(VerifyOptions options, IConsole console);

        public static Parser Create(
            StartServer start,
            Demo demo,
            TryGitHub tryGithub,
            Pack pack,
            Install install,
            Verify verify)
        {
            var startHandler = CommandHandler.Create<InvocationContext, StartupOptions>((context, options) =>
            {
                start(options, context);
            });

            var rootCommand = StartInTryMode();
            rootCommand.Handler = startHandler;

            var startInHostedMode = StartInHostedMode();
            startInHostedMode.Handler = startHandler;

            rootCommand.AddCommand(startInHostedMode);
            rootCommand.AddCommand(Demo());
            rootCommand.AddCommand(ListPackages());
            rootCommand.AddCommand(GitHub());
            rootCommand.AddCommand(Pack());
            rootCommand.AddCommand(Install());
            rootCommand.AddCommand(Verify());

            return new CommandLineBuilder(rootCommand)
                   .UseDefaults()
                   .UseMiddleware(async (context, next) =>
                   {
                       if (context.ParseResult.Directives.Contains("debug") &&
                           !(Clock.Current is VirtualClock))
                       {
                           VirtualClock.Start();
                       }

                       await next(context);
                   })
                   .Build();

            RootCommand StartInTryMode()
            {
                var command = new RootCommand
                              {
                                  Description = "Try out a .NET project with interactive documentation in your browser",
                                  Argument = new Argument<DirectoryInfo>(() => new DirectoryInfo(Directory.GetCurrentDirectory()))
                                             {
                                                 Name = "rootDirectory",
                                                 Description = "Specify the path to the root directory"
                                             }.ExistingOnly()
                              };

                command.AddOption(new Option(
                                     "--add-source",
                                     "Specify an additional nuget package source",
                                     new Argument<DirectoryInfo>(new DirectoryInfo(Directory.GetCurrentDirectory())).ExistingOnly()));

                command.AddOption(new Option(
                     "--uri",
                     "Specify a URL to a markdown file",
                     new Argument<Uri>()));

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
                    var packagesDirectory = new DirectoryInfo(Path.Combine(Package.DefaultPackagesDirectory.FullName, ".store"));

                    if (packagesDirectory.Exists)
                    {
                        foreach (var package in packagesDirectory.GetDirectories())
                        {
                            console.Out.WriteLine(package.FullName);
                        }
                    }
                    else
                    {
                        console.Out.WriteLine("No Try .NET packages installed");
                    }
                });

                return run;
            }

            Command Demo()
            {
                var demoCommand = new Command("demo")
                                  {
                                      new Option("--output", "Where should the demo project be written to?")
                                      {
                                          Argument = new Argument<DirectoryInfo>(
                                              defaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory()))
                                      }
                                  };

                demoCommand.Handler = CommandHandler.Create<DemoOptions, IConsole>((options, console) =>
                {
                    demo(options, console);
                });

                return demoCommand;
            }

            Command GitHub()
            {
                var argument = new Argument<string>();

                // System.CommandLine parameter binding does lookup by name,
                // so name the argument after the github command's string param
                argument.Name = nameof(TryGitHubOptions.Repo);

                var github = new Command("github", "Try a GitHub repo", argument: argument);

                github.Handler = CommandHandler.Create<TryGitHubOptions, IConsole>((repo, console) => tryGithub(repo, console));

                return github;
            }

            Command Pack()
            {
                var packCommand = new Command("pack", "create a package");
                packCommand.Argument = new Argument<DirectoryInfo>();
                packCommand.Argument.Name = "packTarget";

                packCommand.Handler = CommandHandler.Create<PackOptions, IConsole>(
                    (options, console) =>
                    {
                        return pack(options, console);
                    });

                return packCommand;
            }

            Command Install()
            {
                var installCommand = new Command("install", "install a package");
                installCommand.Argument = new Argument<string>();
                installCommand.Argument.Name = nameof(InstallOptions.PackageName);

                var option = new Option("--add-source",
                                        argument: new Argument<DirectoryInfo>().ExistingOnly());

                installCommand.AddOption(option);

                installCommand.Handler = CommandHandler.Create<InstallOptions, IConsole>((options, console) => install(options, console));

                return installCommand;
            }

            Command Verify()
            {
                var verifyCommand = new Command("verify")
                                    {
                                        Argument = new Argument<DirectoryInfo>(() => new DirectoryInfo(Directory.GetCurrentDirectory()))
                                                   {
                                                       Name = nameof(VerifyOptions.RootDirectory),
                                                       Description = "Specify the path to the root directory"
                                                   }.ExistingOnly()
                                    };

                verifyCommand.Handler = CommandHandler.Create<VerifyOptions, IConsole>((options, console) =>
                {
                    return verify(options, console);
                });

                return verifyCommand;
            }
        }
    }
}