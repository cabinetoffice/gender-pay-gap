using System;
using Microsoft.AspNetCore.Http;

namespace GenderPayGap.WebUI.Helpers
{
    public static class HttpContextHelper
    {

        public static void SetResponseCache(HttpContext context, int maxSeconds)
        {
            if (maxSeconds > 0)
            {
                context.Response.Headers["Cache-Control"] = $"public,max-age={maxSeconds}";
            }
            else
            {
                context.Response.Headers["Cache-Control"] = $"no-cache,no-store,max-age=0,must-revalidate";
            }
        }

        public static void SetResponseCookie(HttpContext context,
            string key,
            string value,
            DateTime expires,
            string subdomain = null,
            string path = "/",
            bool httpOnly = false,
            bool secure = false)
        {
            var cookieOptions = new CookieOptions {
                Expires = expires,
                Domain = subdomain,
                Path = path,
                Secure = secure,
                HttpOnly = httpOnly
            };
            if (string.IsNullOrWhiteSpace(value))
            {
                context.Response.Cookies.Delete(key);
            }
            else
            {
                context.Response.Cookies.Append(key, value, cookieOptions);
            }
        }

    }
}
