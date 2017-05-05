using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MLS.Agent
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc()
                    .AddJsonOptions(o =>
                    {
                        o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                        o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseDefaultFiles()
               .UseStaticFiles()
               .UseMvc()

               //               
               //               .AddJsonOptions(opt =>
               //               {
               //                   var resolver  = opt.SerializerSettings.ContractResolver;
               //                   if (resolver != null)
               //                   {
               //                       var res = resolver as DefaultContractResolver;
               //                       res.NamingStrategy = null;  // <<!-- this removes the camelcasing
               //                   }
               .UseDeveloperExceptionPage();
        }
    }
}