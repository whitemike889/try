using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.AspNetCore.Blazor.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MLS.Agent.Blazor;
using MLS.Agent.CommandLine;
using MLS.Agent.Markdown;
using MLS.Agent.Middleware;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pocket;
using Recipes;
using WorkspaceServer;
using WorkspaceServer.Servers.Roslyn;
using static Pocket.Logger<MLS.Agent.Startup>;
using IApplicationLifetime = Microsoft.Extensions.Hosting.IApplicationLifetime;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;
using Process = System.Diagnostics.Process;

namespace MLS.Agent
{
    public class Startup
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable
        {
            () => Logger<Program>.Log.Event("AgentStopping")
        };

        public Startup(
            IHostingEnvironment env,
            StartupOptions startupOptions)
        {
            Environment = env;
            StartupOptions = startupOptions;

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath);

            Configuration = configurationBuilder.Build();

        }

        protected IConfigurationRoot Configuration { get; }

        protected IHostingEnvironment Environment { get; }

        public StartupOptions StartupOptions { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                // Add framework services.
                services.AddMvc(options =>
                        {
                            options.Filters.Add(new ExceptionFilter());
                            options.Filters.Add(new BadRequestOnInvalidModelFilter());
                        })
                        .AddJsonOptions(o =>
                        {
                            o.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                            o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                            o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                        });

                services.AddSingleton(Configuration);

                services.AddSingleton(c => new RoslynWorkspaceServer(c.GetRequiredService<PackageRegistry>()));

                services.TryAddSingleton<IBrowserLauncher>(c => new BrowserLauncher());

                services.AddResponseCompression(options =>
                {
                    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                    {
                        MediaTypeNames.Application.Octet,
                        WasmMediaTypeNames.Application.Wasm
                    });
                });

                if (StartupOptions.IsInHostedMode)
                {
                    services.AddSingleton(_ => PackageRegistry.CreateForHostedMode());
                    services.AddSingleton(c => new MarkdownProject(c.GetRequiredService<PackageRegistry>()));
                    services.AddSingleton<IHostedService, Warmup>();
                }
                else
                {
                    services.AddSingleton(_ => PackageRegistry.CreateForTryMode(StartupOptions.RootDirectory, StartupOptions.AddSource));
                    services.AddSingleton(c => new MarkdownProject(c.GetRequiredService<IDirectoryAccessor>(), c.GetRequiredService<PackageRegistry>()));
                    services.AddSingleton(_ => CreateDirectoryAccessor());
                }

                services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All;
                });

                operation.Succeed();
            }
        }

        private IDirectoryAccessor CreateDirectoryAccessor()
        {
            if (StartupOptions.Uri != null)
            {
                if (StartupOptions.Uri.IsAbsoluteUri)
                {
                    var client = new WebClient();
                    var tempDirPath = Path.Combine(
                        Path.GetTempPath(),
                        Path.GetRandomFileName());

                    var tempDir = Directory.CreateDirectory(tempDirPath);

                    var temp = Path.Combine(
                        tempDir.FullName,
                        Path.GetFileName(StartupOptions.Uri.LocalPath));

                    client.DownloadFile(StartupOptions.Uri, temp);
                    var fileInfo = new FileInfo(temp);
                    return new FileSystemDirectoryAccessor(fileInfo.Directory);
                }
            }

            return new FileSystemDirectoryAccessor(StartupOptions.RootDirectory);
        }

        public void Configure(
            IApplicationBuilder app,
            IApplicationLifetime lifetime,
            IBrowserLauncher browserLauncher)
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                lifetime.ApplicationStopping.Register(() => _disposables.Dispose());

                ConfigureForOrchestratorProxy(app);

                app.Map("/LocalCodeRunner/blazor-console", builder =>
                {
                    builder.UsePathBase("/LocalCodeRunner/blazor-console/");
                    builder.UseBlazor<MLS.Blazor.Program>();
                });

                var budget = new Budget();
                _disposables.Add(() => budget.Cancel());
                BlazorPackageConfiguration.Configure(app, app.ApplicationServices, budget);

                app.UseDefaultFiles()
                    .UseStaticFilesFromToolLocation()
                    .UseRouter(new StaticFilesProxyRouter())
                    .UseMvc();

                operation.Succeed();

                if (!StartupOptions.IsInHostedMode)
                {
                    Clock.Current
                         .Schedule(_ => LaunchBrowser(browserLauncher), TimeSpan.FromSeconds(1));
                }
            }
        }

        private static void ConfigureForOrchestratorProxy(IApplicationBuilder app)
        {
            app.UseForwardedHeaders();

            app.Use(async (context, next) =>
            {
                var forwardedPath = context.Request.Headers["X-Forwarded-PathBase"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedPath))
                {
                    context.Request.Path = forwardedPath + context.Request.Path;
                }

                await next();
            });
        }

        private void LaunchBrowser(IBrowserLauncher browserLauncher)
        {
            var processName = Process.GetCurrentProcess().ProcessName;

            var uri = processName == "dotnet" ||
                      processName == "dotnet.exe"
                          ? new Uri("http://localhost:4242")
                          : new Uri("http://localhost:5000");

            if (StartupOptions.Uri != null &&
                !StartupOptions.Uri.IsAbsoluteUri)
            {
                uri = new Uri(uri, StartupOptions.Uri);
            }

            browserLauncher.LaunchBrowser(uri);
        }
    }
}
