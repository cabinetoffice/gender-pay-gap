using System.Collections.Generic;
using System.Threading.Tasks;
using GenderPayGap.Core;
using Microsoft.AspNetCore.Http;

namespace GenderPayGap.WebUI.Helpers
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
                    foreach (KeyValuePair<string, string> securityHeader in Global.SecurityHeadersToAdd)
                    {
                        if (!httpContext.Response.Headers.ContainsKey(securityHeader.Key))
                        {
                            httpContext.Response.Headers.Add(securityHeader.Key, securityHeader.Value);
                        }
                        else if (httpContext.Response.Headers[securityHeader.Key] != securityHeader.Value)
                        {
                            httpContext.Response.Headers.Remove(securityHeader.Key); // This is required as we cannot change a key once it is added
                            httpContext.Response.Headers[securityHeader.Key] = securityHeader.Value;
                        }
                    }

                    foreach (string securityHeaderName in Global.SecurityHeadersToRemove)
                    {
                        httpContext.Response.Headers.Remove(securityHeaderName);
                    }

                    return Task.CompletedTask;
                });

            await _next.Invoke(httpContext);
        }

    }
}
