using System.Collections.Generic;
using System.Threading.Tasks;
using GenderPayGap.Core;
using Microsoft.AspNetCore.Http;

namespace GenderPayGap.Extensions.AspNetCore
{
    public class SecurityHeaderMiddleware
    {

        private readonly RequestDelegate _next;

        public SecurityHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.OnStarting(
                () => {
                    foreach (KeyValuePair<string,string> securityHeader in Global.SecurityHeaders)
                    {
                        httpContext.SetResponseHeader(securityHeader.Key, securityHeader.Value);
                    }

                    return Task.CompletedTask;
                });

            await _next.Invoke(httpContext);
        }

    }
}
