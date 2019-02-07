using System;
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
                var packageInfo = builder.GetPackageInfo();
                // ideally info would tell if this is something we can blazor
                if (packageInfo.BlazorSupported)
                {
                    var package = builder.GetPackage(budget).Result;
                    // if is a normal nuget package should we emit a special loader to run in blazor?
                    var blazorEntryPoint = package.BlazorEntryPointAssemblyPath;
                    var path = $"/LocalCodeRunner/blazor-{packageInfo.Type}";
                    app.Map(path, a =>
                    {
                        app.UsePathBase(path);
                        // this is will cause the addition of a new static file provider, might cause issues
                        app.UseBlazor(new BlazorOptions{ClientAssemblyPath = blazorEntryPoint.FullName});
                    });
                }
            }
        }
    }
}
