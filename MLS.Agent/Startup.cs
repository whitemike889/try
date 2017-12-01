using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pocket;
using Recipes;
using LoggerConfiguration = Serilog.LoggerConfiguration;
using Serilog.Sinks.RollingFileAlternate;
using static Pocket.Logger<MLS.Agent.Startup>;

namespace MLS.Agent
{
    public class Startup
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

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
            // Add framework services.
            services.AddMvc(options => options.Filters.Add(new ExceptionFilter()))
                    .AddJsonOptions(o =>
                    {
                        o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                        o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    });

            services.AddSingleton(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (Environment.IsDevelopment())
            {
                var log = new LoggerConfiguration()
                    .WriteTo
                    .RollingFileAlternate("logs", outputTemplate: "{Message}{NewLine}")
                    .CreateLogger();

                var subscription = LogEvents.Subscribe(e => log.Information(e.ToLogString()));

                _disposables.Add(subscription);
                _disposables.Add(log);
            }

            Log.Info("Agent version {orchestrator_version} starting in environment {environment}",
                     AssemblyVersionSensor.Version().AssemblyInformationalVersion,
                     Environment.EnvironmentName);

            app.UseDefaultFiles()
               .UseStaticFiles()
               .UseMvc();
        }
    }
}
