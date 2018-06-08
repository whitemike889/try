using System;
using Clockwise;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MLS.Agent.JsonContracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pocket;
using Recipes;
using WorkspaceServer.Servers.InMemory;
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
                            o.SerializerSettings.Converters.Add(new WorkspaceRequestConverter());
                        });

                services.AddSingleton(Configuration);

                services.AddSingleton(_ => DefaultWorkspaces.CreateWorkspaceServerRegistry());
                services.AddSingleton(_ => new InMemoryWorkspaceServer());

                services.AddSingleton<IHostedService, Warmup>();

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

                var budget = new Budget();

                _disposables.Add(() => budget.Cancel());

                operation.Succeed();
            }
        }
    }
}


