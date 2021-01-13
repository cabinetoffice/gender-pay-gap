using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GenderPayGap.WebUI.Helpers
{
    public class NoCacheMiddleware
    {
        private readonly RequestDelegate _next;

        public NoCacheMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.OnStarting(
                () =>
                {
                    httpContext.Response.Headers["Cache-Control"] = "no-cache";
                    return Task.CompletedTask;
                });

            return this._next(httpContext);
        }
    }
}
