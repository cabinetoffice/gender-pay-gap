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
            //Redirect to holding mage if in maintenance mode
            if (_enabled && !httpContext.GetUri().PathAndQuery.StartsWithI(@"/error/service-unavailable"))
            {
                httpContext.Response.Redirect(@"/error/service-unavailable", true);
            }

            await _next.Invoke(httpContext);
        }

    }
}
