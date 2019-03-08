using System;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WorkspaceServer;
using WorkspaceServer.Packaging;

namespace MLS.Agent.Blazor
{
    internal sealed class BlazorPackageConfiguration
    {
        public static void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, Budget budget)
        {
            var registry = serviceProvider.GetService<PackageRegistry>();

            foreach (var builderFactory in registry)
            {
                var builder = builderFactory.Result;
                if (builder.BlazorSupported)
                {
                    app.Use(async (context, next) =>
                    {
                        var requestPath = context.Request.Path;
                        if (requestPath.StartsWithSegments($"/LocalCodeRunner/{builder.PackageName.Remove(0, "runner".Length)}"))
                        {
                            var package = builder.GetPackage() as BlazorPackage;
                            await package.EnsurePublished();
                        }
                    })

                    //var package = builder.GetPackage() as BlazorPackage;
                    //var readyTask = Task.Run(package.Prepare);
                    //readyTask.Wait();
                    //app.Map(package.CodeRunnerPath, appBuilder =>
                    //{
                    //    var blazorEntryPoint = package.BlazorEntryPointAssemblyPath;
                    //    appBuilder.UsePathBase(package.CodeRunnerPathBase);
                    //    appBuilder.UseBlazor(new BlazorOptions { ClientAssemblyPath = blazorEntryPoint.FullName });
                    //});
                }
            }
        }
    }
}
