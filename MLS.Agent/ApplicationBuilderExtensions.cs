using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace MLS.Agent
{
    internal static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseStaticFilesFromToolLocation(this IApplicationBuilder app)
        {
            var options = GetStaticFilesOptions();

            if (options != null)
            {
                app.UseSpaStaticFiles(options);
            }
            else
            {
                app.UseStaticFiles();
            }

            return app;
        }

        private static StaticFileOptions GetStaticFilesOptions()
        {
            var paths = new List<string>
            {
                Path.Combine(Path.GetDirectoryName(typeof(Startup).Assembly.Location), "wwwroot")
            };


            var providers = paths.Where(Directory.Exists).Select(p => new PhysicalFileProvider(p)).ToArray();


            StaticFileOptions options = null;

            if (providers.Length > 0)
            {
                var combinedProvider = new CompositeFileProvider(providers);

                var sharedOptions = new SharedOptions { FileProvider = combinedProvider };
                options = new StaticFileOptions(sharedOptions);
            }

            return options;
        }
    }
}