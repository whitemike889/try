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
                if (builder.BlazorSupported)
                {
                    // if is a normal nuget package should we emit a special loader to run in blazor?
                    var path = $"/LocalCodeRunner/blazor-{builder.PackageName}";
                    app.Map(path, async a =>
                    {
                        var package = await builder.GetPackage();
                        var blazorEntryPoint = package.BlazorEntryPointAssemblyPath;
                        app.UsePathBase(path);
                        // this is will cause the addition of a new static file provider, might cause issues
                        app.UseBlazor(new BlazorOptions { ClientAssemblyPath = blazorEntryPoint.FullName });
                    });
                }
            }
        }
    }
}
