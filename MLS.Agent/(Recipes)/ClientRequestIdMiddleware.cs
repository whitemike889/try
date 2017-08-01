using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace ClientRequestIdMiddleware
{
    public class ClientRequestIdMiddleware
    {
        private static readonly AsyncLocal<string> _clientRequestId = new AsyncLocal<string>();
        private readonly RequestDelegate _next;
        private readonly string _headerName;

        public static string ClientRequestId
        {
            get => _clientRequestId.Value;
            private set => _clientRequestId.Value = value;
        }

        public ClientRequestIdMiddleware(RequestDelegate next, IOptions<Options> options = null)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));

            _headerName = options?.Value?.HeaderName ?? "client-request-id";
        }

        public Task Invoke(HttpContext context)
        {
            var correlationIds = context.Request.Headers.TryGetValue(_headerName, out StringValues headerValues)
                ? headerValues.ToArray() 
                : new [] { Guid.NewGuid().ToString() };

            ClientRequestId = string.Join(",", correlationIds);
 
            context.Response.OnStarting(() => 
            { 
                context.Response.Headers.Add(_headerName, correlationIds);
                
                return Task.CompletedTask; 
            }); 
 
            return _next(context); 
        }
        
 
        public class Options 
        { 
            public string HeaderName { get; set; }
        } 
    }
    
    public static class ClientRequestIdExtensions
    {
        public static IApplicationBuilder UseClientRequestId(this IApplicationBuilder app, string header = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<ClientRequestIdMiddleware>(Options.Create(new ClientRequestIdMiddleware.Options { HeaderName = header }));
        }
    }
}