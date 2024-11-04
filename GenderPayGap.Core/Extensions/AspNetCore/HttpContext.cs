using Microsoft.AspNetCore.Http;

namespace System.Web
{
    public static class HttpContext
    {

        public static void SetResponseCache(this Microsoft.AspNetCore.Http.HttpContext context, int maxSeconds)
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

        public static string GetRequestCookieValue(this Microsoft.AspNetCore.Http.HttpContext context, string key)
        {
            return context.Request.Cookies.ContainsKey(key) ? context.Request.Cookies[key] : null;
        }

        public static void SetResponseCookie(this Microsoft.AspNetCore.Http.HttpContext context,
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
