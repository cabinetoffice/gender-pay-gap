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
            _securityHeaders = Global.SecurityHeaders;
        }

        private Dictionary<string, string> _securityHeaders { get; }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.OnStarting(
                () => {
                    foreach (string key in _securityHeaders.Keys)
                    {
                        string value = _securityHeaders[key];

                        //Lookup the same value from another header
                        string varName = value.GetVariableName();
                        if (!string.IsNullOrWhiteSpace(varName) && _securityHeaders.ContainsKey(varName))
                        {
                            value = _securityHeaders[varName];
                        }

                        httpContext.SetResponseHeader(key, value);
                    }

                    return Task.CompletedTask;
                });

            await _next.Invoke(httpContext);
        }

    }
}
