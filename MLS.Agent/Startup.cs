using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MLS.Agent.Markdown;
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

                if (StartupOptions.IsInHostedMode)
                {
                    services.AddSingleton(_ => PackageRegistry.CreateForHostedMode());
                    services.AddSingleton<IHostedService, Warmup>();
                }
                else
                {
                    services.AddSingleton(_ => PackageRegistry.CreateForTryMode(StartupOptions.RootDirectory, StartupOptions.AddSource));
                    services.AddSingleton(c => new MarkdownProject(c.GetRequiredService<IDirectoryAccessor>(), c.GetRequiredService<PackageRegistry>()));
                    services.AddSingleton((Func<IServiceProvider, IDirectoryAccessor>)(_ => CreateDirectoryAccessor()));
                }

                operation.Succeed();
            }
        }

        private IDirectoryAccessor CreateDirectoryAccessor()
        {
            if (StartupOptions.Uri != null)
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

            return new FileSystemDirectoryAccessor(StartupOptions.RootDirectory);
        }

        public void Configure(
            IApplicationBuilder app,
            IApplicationLifetime lifetime)
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                lifetime.ApplicationStopping.Register(() => _disposables.Dispose());

                app.UseDefaultFiles()
                    .UseStaticFilesFromToolLocation()
                    .UseRouter(new StaticFilesProxyRouter())
                    .UseMvc();
                
                var budget = new Budget();

                _disposables.Add(() => budget.Cancel());

                operation.Succeed();

                if (!StartupOptions.IsInHostedMode &&
                    Environment.EnvironmentName != "test" &&
                    !Debugger.IsAttached)
                {
                    Task.Delay(TimeSpan.FromSeconds(1))
                        .ContinueWith(task =>
                        {
                            var url = "http://localhost:4242";
                         
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
                            }
                            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                            {
                                Process.Start("xdg-open", url);
                            }
                            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                            {
                                Process.Start("open", url);
                            }
                        });
                }
            }
        }


    }
}
