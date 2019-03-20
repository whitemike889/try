using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace MLS.Blazor
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
