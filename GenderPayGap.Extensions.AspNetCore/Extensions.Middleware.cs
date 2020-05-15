using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using HttpContext = System.Web.HttpContext;

namespace GenderPayGap.Extensions.AspNetCore
{
    public static partial class Extensions
    {

        public static IApplicationBuilder UseStaticHttpContext(this IApplicationBuilder app)
        {
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            HttpContext.Configure(httpContextAccessor);
            return app;
        }

        public static IApplicationBuilder UseSecurityHeaderMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeaderMiddleware>();
        }
        
        public static IApplicationBuilder UseMaintenancePageMiddleware(this IApplicationBuilder builder, bool enable)
        {
            return builder.UseMiddleware<MaintenancePageMiddleware>(enable);
        }

    }
}
