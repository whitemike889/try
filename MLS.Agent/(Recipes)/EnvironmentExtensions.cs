using Microsoft.AspNetCore.Hosting;

namespace Recipes
{
    public static class EnvironmentExtensions
    {
        public static bool IsTest(this IHostingEnvironment hostingEnvironment) =>
            hostingEnvironment.EnvironmentName == "test";

        public static IWebHostBuilder UseTestEnvironment(this IWebHostBuilder builder) =>
            builder.UseEnvironment("test");
    }
}
