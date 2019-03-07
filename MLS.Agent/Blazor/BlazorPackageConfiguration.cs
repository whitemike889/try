using System;
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
                    var package = builder.GetPackage() as BlazorPackage;
                    var readyTask = package.CreateRoslynWorkspaceForRunAsync(budget);

                    // Todo: don't do this in server startup path
                    readyTask.Wait();
                    app.Map(package.CodeRunnerPath, appBuilder =>
                    {
                        var blazorEntryPoint = package.BlazorEntryPointAssemblyPath;
                        appBuilder.UsePathBase(package.CodeRunnerPathBase);
                        appBuilder.UseBlazor(new BlazorOptions { ClientAssemblyPath = blazorEntryPoint.FullName });
                    });
                }
            }
        }
    }
}
