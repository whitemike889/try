using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Pocket;
using static Pocket.Logger<Recipes.ExceptionFilter>;

namespace Recipes
{
    internal class ExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception == null)
            {
                return;
            }

            context.Result = new ExceptionResult(context.Exception);

            if (context.ExceptionHandled)
            {
                Log.Warning(context.Exception);
            }
            else
            {
                Log.Error(context.Exception);
            }
        }

        private class ExceptionResult : IActionResult
        {
            private readonly Exception exception;

            public ExceptionResult(Exception exception)
            {
                this.exception = exception;
            }

            public async Task ExecuteResultAsync(ActionContext context)
            {
                var objectResult = new ObjectResult(new
                {
                    message = "An unhandled exception occurred.",
                    exception = exception.ToString()
                })
                {
                    StatusCode = 500
                };

                await objectResult.ExecuteResultAsync(context);
            }
        }
    }
}
