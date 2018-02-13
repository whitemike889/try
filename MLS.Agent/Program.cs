using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using static Pocket.Logger<MLS.Agent.Program>;
using Pocket;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MLS.Agent.Tools;
using Pocket.For.ApplicationInsights;
using Recipes;
using Serilog.Sinks.RollingFileAlternate;
using WorkspaceServer;
using WorkspaceServer.Servers.OmniSharp;
using SerilogLoggerConfiguration = Serilog.LoggerConfiguration;

namespace MLS.Agent
{
    public class Program
    {
        public static X509Certificate2 ParseKey(string base64EncodedKey)
        {
            var bytes = Convert.FromBase64String(base64EncodedKey);
            return new X509Certificate2(bytes);
        }

        private static readonly Assembly[] assembliesEmittingPocketLoggerLogs = {
            typeof(Startup).Assembly,
            typeof(Dotnet).Assembly,
            typeof(DotnetWorkspaceServer).Assembly
        };

        private static void StartLogging(CompositeDisposable disposables, CommandLineOptions options)
        {
            var instrumentationKey = options.IsProduction
                                         ? "1bca19cc-3417-462c-bb60-7337605fee38"
                                         : "6c13142c-8ddf-4335-b857-9d3e0cbb1ea1";

            if (options.IsProduction)
            {
                var applicationVersion = AssemblyVersionSensor.Version().AssemblyInformationalVersion;
                var websiteSiteName = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") ?? "UNKNOWN-AGENT";

                disposables.Add(
                    LogEvents.Enrich(a =>
                    {
                        a(("applicationVersion", applicationVersion));
                        a(("websiteSiteName", websiteSiteName));
                    }));
            }
            else
            {
                var log = new SerilogLoggerConfiguration()
                          .WriteTo
                          .RollingFileAlternate("./logs", outputTemplate: "{Message}{NewLine}")
                          .CreateLogger();

                var subscription = LogEvents.Subscribe(
                    e => log.Information(e.ToLogString()),
                    assembliesEmittingPocketLoggerLogs);

                disposables.Add(subscription);
                disposables.Add(log);

                disposables.Add(LogEvents.Subscribe(e => Console.WriteLine(e.ToLogString())));
            }

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Log.Warning($"{nameof(TaskScheduler.UnobservedTaskException)}", args.Exception);
                args.SetObserved();
            };

            var telemetryClient = new TelemetryClient(new TelemetryConfiguration(instrumentationKey))
            {
                InstrumentationKey = options.ApplicationInsightsKey ?? instrumentationKey
            };

            disposables.Add(telemetryClient.SubscribeToPocketLogger());

            Log.Event("AgentStarting");
        }

        public static IWebHost ConstructWebHost(CommandLineOptions options)
        {
            var disposables = new CompositeDisposable();
            StartLogging(disposables, options);

            if (options.Key is null)
            {
                Log.Info("No Key Provided");
            }
            else
            {
                Log.Info("Received Key", options.Key);
            }
            
            var webHost = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .ConfigureServices(c =>
                {
                    c.AddSingleton(options);

                    c.AddSingleton(_ =>
                    {
                        var registry = new WorkspaceServerRegistry();
                        registry.AddWorkspace("console",
                                            workspace =>
                                            {
                                                workspace.CreateUsingDotnet("console");
                                            });
                        registry.AddWorkspace("Twilio.Demo",
                                            workspace =>
                                            {
                                                workspace.CreateUsingDotnet("console");
                                                workspace.AddPackageReference("Twilio", "5.9.2");
                                            });
                        return registry;
                    });
                })
                .UseStartup<Startup>()
                .Build();

            return webHost;
        }

        public static int Main(string[] args)
        {
            var options = CommandLineOptions.Parse(args);
            if (options.HelpRequested)
            {
                Console.WriteLine(options.HelpText);
                return 0;
            }

            if (!options.WasSuccess)
            {
                Console.WriteLine(options.HelpText);
                return 1;
            }

            ConstructWebHost(options).Run();
            return 0;
        }
    }
}
