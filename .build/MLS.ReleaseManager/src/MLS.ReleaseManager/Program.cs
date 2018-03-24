using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace MLS.ReleaseManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var tenantId = args[0];

            var subscriptionId = args[1];

            var resourceGroupName = args[2];

            var webAppName = args[3];

            var clientId = args[4];

            var clientSecret = args[5];

            RestartWebApp(tenantId, subscriptionId, resourceGroupName, webAppName, clientId, clientSecret)
                .Wait();
        }

        private static async Task RestartWebApp(string tenantId, string subscriptionId, string resourceGroupName,
            string webAppName, string clientId, string clientSecret)
        {

             var _restClient = RestClient.Configure()
                .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                .WithCredentials(new AzureCredentials(new ServicePrincipalLoginInformation
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    },
                    tenantId,
                    AzureEnvironment.AzureGlobalCloud))
                .Build();

            var _appServiceManager =
                new AppServiceManager(_restClient, subscriptionId, tenantId);

                const int timeout = 30;

                _appServiceManager.WebApps
                    .GetByResourceGroup(resourceGroupName, webAppName)
                    .Stop();


            await _appServiceManager
                .WaitForStateTransition(resourceGroupName, webAppName, "Stopped", timeout);

            _appServiceManager.WebApps
                .GetByResourceGroup(resourceGroupName, webAppName)
                .Start();

            await _appServiceManager
                .WaitForStateTransition(resourceGroupName, webAppName, "Running", timeout);
        }
    }

    public static class IAppServiceManagerExtensions
    {
        public static async Task WaitForStateTransition(this IAppServiceManager subject, string resourceGroupName, string webAppName, string targetState, int timeout)
        {
            var throwAfter = DateTimeOffset.Now.AddSeconds(timeout);

            while (true)
            {
                var state = (await subject.WebApps
                        .GetByResourceGroupAsync(resourceGroupName, webAppName))
                    .State;
                if (state == targetState)
                {
                    break;
                }

                if (DateTimeOffset.Now > throwAfter)
                {
                    throw new Exception($"Web app failed to stop within {timeout} seconds.");
                }

                Console.WriteLine($"In state {state}");

                await Task.Delay(1000);
            }
        }
    }
}
