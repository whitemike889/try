using Microsoft.AspNetCore.Blazor.Hosting;
using System.ComponentModel;

namespace MLS.HumanizerRunner
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebAssemblyHostBuilder CreateHostBuilder(string[] args) =>
            BlazorWebAssemblyHost.CreateDefaultBuilder()
                .UseBlazorStartup<Startup>();
    }
}
