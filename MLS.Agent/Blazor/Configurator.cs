using System;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WorkspaceServer;

namespace MLS.Agent.Blazor
{
    internal sealed class Configurator
    {
        public static void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, Budget budget = null)
        {
            var registry = serviceProvider.GetService<PackageRegistry>();

            foreach (var builderFactory in registry)
            {
                var builder = builderFactory.Result;
                if (builder.BlazorSupported)
                {
                    var package = builder.GetPackage().Result;
                    var readyTask = package.EnsureReady(budget);
                    readyTask.Wait();
                    var name = builder.PackageName.Remove(0, "runner-".Length);
                    //var name = builder.PackageName;
                    var path = $"/LocalCodeRunner/{name}";
                    app.Map(path, appBuilder =>
                    {
                        var blazorEntryPoint = package.BlazorEntryPointAssemblyPath;
                        appBuilder.UsePathBase(path + "/");
                        // this is will cause the addition of a new static file provider, might cause issues
                        appBuilder.UseBlazor(new BlazorOptions { ClientAssemblyPath = blazorEntryPoint.FullName });
                    });
                }
            }
        }
    }
}
