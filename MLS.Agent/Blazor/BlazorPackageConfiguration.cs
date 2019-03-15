using System;
using System.Collections.Generic;
using System.Linq;
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

            List<Task> prepareTasks = new List<Task>();

            foreach (var builderFactory in registry)
            {
                var builder = builderFactory.Result;
                if (builder.BlazorSupported)
                {
                    var package = builder.GetPackage() as BlazorPackage;
                    if (PackageHasBeenBuiltAndHasBlazorStuff(package))
                    {
                        SetupMappingsForBlazorContentsOfPackage(package, app);
                    }
                    else
                    {
                        prepareTasks.Add(Task.Run(package.Prepare).ContinueWith(t => {
                            if (t.IsCompletedSuccessfully)
                            {
                                SetupMappingsForBlazorContentsOfPackage(package, app);
                            }
                        }));
                    }
                    //var readyTask = Task.Run(package.Prepare);
                    //readyTask.Wait();
                   
                }
            }

            Task.WaitAll(prepareTasks.ToArray());
        }

        private static void SetupMappingsForBlazorContentsOfPackage(BlazorPackage package, IApplicationBuilder builder)
        {
            builder.Map(package.CodeRunnerPath, appBuilder =>
            {
                var blazorEntryPoint = package.BlazorEntryPointAssemblyPath;
                appBuilder.UsePathBase(package.CodeRunnerPathBase);
                appBuilder.UseBlazor(new BlazorOptions { ClientAssemblyPath = blazorEntryPoint.FullName });
            });
        }

        private static bool PackageHasBeenBuiltAndHasBlazorStuff(BlazorPackage package)
        {
            return package.BlazorEntryPointAssemblyPath.Exists;
        }
    }
}
