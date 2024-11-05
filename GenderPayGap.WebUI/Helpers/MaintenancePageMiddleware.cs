using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace GenderPayGap.WebUI.Helpers
{
    public class MaintenancePageMiddleware
    {

        private readonly bool _enabled;
        private readonly RequestDelegate _next;

        public MaintenancePageMiddleware(RequestDelegate next, bool enabled)
        {
            _next = next;
            _enabled = enabled;
        }


        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.Value.StartsWith("/health-check"))
            {
                await _next.Invoke(httpContext);
                return;
            }

            // Redirect to holding page if in maintenance mode
            if (_enabled && !httpContext.Request.Path.Value.StartsWith(@"/error/service-unavailable", StringComparison.InvariantCultureIgnoreCase))
            {
                httpContext.Response.Redirect(@"/error/service-unavailable", permanent: false);
            }

            await _next.Invoke(httpContext);
        }

    }
}
