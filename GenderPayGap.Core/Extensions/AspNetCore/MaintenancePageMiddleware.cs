using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace GenderPayGap.Extensions.AspNetCore
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
            if (System.Web.HttpContext.GetUri(httpContext).PathAndQuery.StartsWith("/health-check"))
            {
                await _next.Invoke(httpContext);
                return;
            }

            // Redirect to holding page if in maintenance mode
            if (_enabled && !httpContext.GetUri().PathAndQuery.StartsWithI(@"/error/service-unavailable"))
            {
                httpContext.Response.Redirect(@"/error/service-unavailable", permanent: false);
            }

            await _next.Invoke(httpContext);
        }

    }
}
