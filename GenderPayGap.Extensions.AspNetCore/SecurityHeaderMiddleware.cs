using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace GenderPayGap.Extensions.AspNetCore
{
    public class SecurityHeaderMiddleware
    {

        private readonly RequestDelegate _next;

        public SecurityHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
            _securityHeaders = GetSecurityHeaders();
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

        public static Dictionary<string, string> GetSecurityHeaders()
        {
            var securityHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            //Load the Security headers from the config file
            Config.Configuration.GetSection("SecurityHeaders").Bind(securityHeaders);

            return securityHeaders;
        }

    }
}
