using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MLS.Repositories
{
    public class PackageLocator
    {
        public async Task LocatePackageAsync(string name)
        {
            var settings = NuGet.Configuration.Settings.LoadDefaultSettings(Directory.GetCurrentDirectory());
            IPackageSourceProvider psp = new PackageSourceProvider(settings);
            ISourceRepositoryProvider srp = new SourceRepositoryProvider(psp, FactoryExtensionsV3.GetCoreV3(Repository.Provider));

            List<IEnumerableAsync<IPackageSearchMetadata>> lists = new List<IEnumerableAsync<IPackageSearchMetadata>>();
            foreach (var source in psp.LoadPackageSources())
            {
                try
                {
                    var thing = Repository.Factory.GetCoreV3(source.Source);
                    var feed = await thing.GetResourceAsync<ListResource>(CancellationToken.None);
                    var result = await feed.ListAsync(name, prerelease: false, allVersions: false, includeDelisted: false, log: new Logger(), token: CancellationToken.None);
                    lists.Add(result);
                }
                catch (FatalProtocolException)
                {
                }

            }

            List<IPackageSearchMetadata> packages = new List<IPackageSearchMetadata>();
            foreach (var list in lists)
            {
                try
                {
                    var e = list.GetEnumeratorAsync();
                    while (await e.MoveNextAsync())
                    {
                        packages.Add(e.Current);
                    }
                }
                catch (FatalProtocolException)
                {

                }
            }
            
            if (packages[0].Title == name)
            {
                Console.WriteLine(packages[0].ProjectUrl);
                return;
            }

            if (packages.Count > 1)
            {
                Console.WriteLine("Which did you mean?");
                foreach (var package in packages)
                {
                    Console.WriteLine($"\t{package.Title}");
                }
            }

            return;
        }

        public class Logger : ILogger
        {

            public void LogInformationSummary(string data)
            {
                Console.WriteLine(data);
            }

            public void Log(LogLevel level, string data)
            {
                Console.WriteLine(data);
            }

            public Task LogAsync(LogLevel level, string data)
            {
                Console.WriteLine(data);
                return Task.CompletedTask;
            }

            public void Log(ILogMessage message)
            {
                Console.WriteLine(message.Message);
            }

            public Task LogAsync(ILogMessage message)
            {
                Console.WriteLine(message.Message);
                return Task.CompletedTask;
            }

            public void LogDebug(string data)
            {
                Console.WriteLine(data);
            }

            public void LogVerbose(string data)
            {
                Console.WriteLine(data);
            }

            public void LogInformation(string data)
            {
                Console.WriteLine(data);
            }

            public void LogMinimal(string data)
            {
                Console.WriteLine(data);
            }

            public void LogWarning(string data)
            {
                Console.WriteLine(data);
            }

            public void LogError(string data)
            {
                Console.WriteLine(data);
            }
        }
    }
}
