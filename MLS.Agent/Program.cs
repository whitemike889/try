using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using static Pocket.Logger<MLS.Agent.Program>;
using Pocket;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Pocket.For.ApplicationInsights;

namespace MLS.Agent
{
    public class Program
    {
        public static X509Certificate2 ParseKey(string base64EncodedKey)
        {
            var bytes = Convert.FromBase64String(base64EncodedKey);
            return new X509Certificate2(bytes);
        }

        private static void StartLogging(CompositeDisposable disposables, CommandLineOptions options)
        {
            var instrumentationKey = options.IsProduction ?
                "1bca19cc-3417-462c-bb60-7337605fee38" :
                "6c13142c-8ddf-4335-b857-9d3e0cbb1ea1";

            var applicationVersion = Recipes.AssemblyVersionSensor.Version().AssemblyInformationalVersion;
            var websiteSiteName = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") ?? "UNKNOWN-AGENT";

            disposables.Add(
                LogEvents.Enrich(a =>
                {
                    a(("applicationVersion", applicationVersion));
                    a(("websiteSiteName", websiteSiteName));
                }));

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Log.Warning($"{nameof(TaskScheduler.UnobservedTaskException)}", args.Exception);
                args.SetObserved();
            };

            //TODO: Re-add serilog logging
            var telemetryClient = new TelemetryClient(new TelemetryConfiguration(instrumentationKey));
            telemetryClient.InstrumentationKey = instrumentationKey;

            disposables.Add(telemetryClient.SubscribeToPocketLogger());

            if (!options.IsProduction)
            {
                disposables.Add(LogEvents.Subscribe(e => Console.WriteLine(e.ToLogString())));
            }

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
