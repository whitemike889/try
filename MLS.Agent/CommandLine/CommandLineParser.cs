using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MLS.Agent.Markdown;
using MLS.Jupyter;
using WorkspaceServer;
using WorkspaceServer.Packaging;
using CommandHandler = System.CommandLine.Invocation.CommandHandler;

namespace MLS.Agent.CommandLine
{
    public static class CommandLineParser
    {
        public delegate void StartServer(
            StartupOptions options,
            InvocationContext context);

        public delegate Task Demo(
            DemoOptions options,
            IConsole console,
            StartServer startServer = null,
            InvocationContext invocationContext = null);

        public delegate Task TryGitHub(
            TryGitHubOptions options,
            IConsole console);

        public delegate Task Pack(
            PackOptions options,
            IConsole console);

        public delegate Task Install(
            InstallOptions options,
            IConsole console);

        public delegate Task<int> Verify(
            VerifyOptions options,
            IConsole console);

        public delegate Task<int> Jupyter(
            JupyterOptions options,
            IConsole console,
            StartServer startServer = null,
            InvocationContext context = null);

        public static Parser Create(
            StartServer startServer,
            Demo demo,
            TryGitHub tryGithub,
            Pack pack,
            Install install,
            Verify verify,
            Jupyter jupyter,
            IServiceCollection services = null)
        {
            services = services ?? new ServiceCollection();

            var rootCommand = StartInTryMode();

            rootCommand.AddCommand(StartInHostedMode());
            rootCommand.AddCommand(Demo());
            rootCommand.AddCommand(ListPackages());
            rootCommand.AddCommand(GitHub());
            rootCommand.AddCommand(Pack());
            rootCommand.AddCommand(Install());
            rootCommand.AddCommand(Verify());
            rootCommand.AddCommand(Jupyter());

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
                    Name = "dotnet-try",
                    Description = ".NET interactive documentation in your browser",
                    Argument = new Argument<DirectoryInfo>(() => new DirectoryInfo(Directory.GetCurrentDirectory()))
                    {
                        Name = nameof(StartupOptions.RootDirectory),
                        Description = "Specify the path to the root directory to run samples from"
                    }.ExistingOnly()
                };

                command.AddOption(new Option(
                                     "--add-source",
                                     "Specify an additional NuGet package source",
                                     new Argument<DirectoryInfo>(new DirectoryInfo(Directory.GetCurrentDirectory())).ExistingOnly()));

                command.AddOption(new Option(
                     "--uri",
                     "Specify a URL to a markdown file",
                     new Argument<Uri>()));

                command.AddOption(new Option(
                    "--enable-preview-features",
                    "Enables preview features",
                    new Argument<bool>()));

                command.Handler = CommandHandler.Create<InvocationContext, StartupOptions>((context, options) =>
                {
                    services.AddSingleton(_ => PackageRegistry.CreateForTryMode(
                                              options.RootDirectory,
                                              options.AddSource));
                 
                    startServer(options, context);
                });

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

                command.Handler = CommandHandler.Create<InvocationContext, StartupOptions>((context, options) =>
                {
                    services.AddSingleton(_ => PackageRegistry.CreateForHostedMode());
                    services.AddSingleton(c => new MarkdownProject(c.GetRequiredService<PackageRegistry>()));
                    services.AddSingleton<IHostedService, Warmup>();

                    startServer(options, context);
                });

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
                var demoCommand = new Command("demo", "Learn how to create Try .NET content with an interactive demo")
                                  {
                                      new Option("--output", "Where should the demo project be written to?")
                                      {
                                          Argument = new Argument<DirectoryInfo>(
                                              defaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory()))
                                      },
                                      new Option("--enable-preview-features", "Enables preview features",
                                          new Argument<bool>())
            };

                demoCommand.Handler = CommandHandler.Create<DemoOptions, InvocationContext>((options, context) =>
                {
                    demo(options, context.Console, startServer, context);
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
            
            Command Jupyter()
            {
                var jupyterCommand = new Command("kernel", "Starts dotnet try as jupyter kernel");
                var connectionFileArgument = new Argument<FileInfo>
                                             {
                                                 Name = "ConnectionFile"
                                             }.ExistingOnly();
                jupyterCommand.Argument = connectionFileArgument;

                jupyterCommand.Handler = CommandHandler.Create<JupyterOptions, IConsole, InvocationContext>((options, console, context) =>
                {
                    services
                        .AddSingleton(c => ConnectionInformation.Load(options.ConnectionFile))
                        .AddSingleton(
                            c =>
                            {
                                return CommandScheduler
                                    .Create<JupyterRequestContext>(delivery => c.GetRequiredService<ICommandHandler<JupyterRequestContext>>()
                                                                                .Trace()
                                                                                .Handle(delivery));
                            })
                        .AddSingleton(c => new JupyterRequestContextHandler(c.GetRequiredService<PackageRegistry>()).Trace())
                        .AddSingleton<IHostedService, Shell>()
                        .AddSingleton<IHostedService, Heartbeat>();

                    return jupyter(options, console, startServer, context);
                });

                return jupyterCommand;
            }

            Command Pack()
            {
                var packCommand = new Command("pack", "Create a Try .NET package");
                packCommand.Argument = new Argument<DirectoryInfo>();
                packCommand.Argument.Name = nameof(PackOptions.PackTarget);

                packCommand.Handler = CommandHandler.Create<PackOptions, IConsole>(
                    (options, console) =>
                    {
                        return pack(options, console);
                    });

                return packCommand;
            }

            Command Install()
            {
                var installCommand = new Command("install", "Install a Try .NET package");
                installCommand.Argument = new Argument<string>();
                installCommand.Argument.Name = nameof(InstallOptions.PackageName);
                installCommand.IsHidden = true;

                var option = new Option("--add-source",
                                        argument: new Argument<DirectoryInfo>().ExistingOnly());

                installCommand.AddOption(option);

                installCommand.Handler = CommandHandler.Create<InstallOptions, IConsole>((options, console) => install(options, console));

                return installCommand;
            }

            Command Verify()
            {
                var verifyCommand = new Command("verify", "Verify Markdown files in the target directory and its children.")
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