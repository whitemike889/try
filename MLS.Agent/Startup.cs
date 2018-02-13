using System;
using System.Threading;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pocket;
using Recipes;
using WorkspaceServer;
using static Pocket.Logger<MLS.Agent.Startup>;

namespace MLS.Agent
{
    public class Startup
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable
        {
            () => Logger<Program>.Log.Event("AgentStopping")
        };

        public Startup(IHostingEnvironment env)
        {
            Environment = env;

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath);

            Configuration = configurationBuilder.Build();
        }

        protected IConfigurationRoot Configuration { get; }

        protected IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                services.AddApplicationInsightsTelemetry(
                    Program.GetInstrumentationKey(
                        Environment.IsProduction()));

                // Add framework services.
                services.AddMvc(options =>
                        {
                            options.Filters.Add(new ExceptionFilter());
                            options.Filters.Add(new BadRequestOnInvalidModelFilter());
                        })
                        .AddJsonOptions(o =>
                        {
                            o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                            o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                        });

                services.AddSingleton(Configuration);

                services.AddSingleton(_ => DefaultWorkspaces.CreateWorkspaceServerRegistry());

                operation.Succeed();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IApplicationLifetime lifetime,
            IHostingEnvironment env,
            IServiceProvider serviceProvider)
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                lifetime.ApplicationStopping.Register(() => _disposables.Dispose());

                app.UseDefaultFiles()
                   .UseStaticFiles()
                   .UseMvc();

                var budget = TimeBudget.Unlimited();

                _disposables.Add(() => budget.Cancel());

                var workspaceServerRegistry = serviceProvider.GetRequiredService<WorkspaceServerRegistry>();

                Task.Factory
                    .StartNew(() => workspaceServerRegistry.StartAllServers(budget),
                              CancellationToken.None,
                              TaskCreationOptions.LongRunning,
                              TaskScheduler.Default);

                operation.Succeed();
            }
        }
    }
}
