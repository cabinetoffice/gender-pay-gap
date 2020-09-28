using GenderPayGap.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;

namespace System.Web
{
    public static class HttpContext
    {

        private static IHttpContextAccessor _contextAccessor;

        public static Microsoft.AspNetCore.Http.HttpContext Current => _contextAccessor?.HttpContext;

        public static void Configure(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public static void DisableResponseCache(this Microsoft.AspNetCore.Http.HttpContext context)
        {
            SetResponseCache(context, 0);
        }

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

        public static string GetParams(this Microsoft.AspNetCore.Http.HttpContext context, string key)
        {
            StringValues? param = context.Request?.Query[key];
            if (string.IsNullOrWhiteSpace(param))
            {
                param = context.Request?.Form[key];
            }

            return param;
        }

        public static string GetBrowser(this Microsoft.AspNetCore.Http.HttpContext context)
        {
            return context.Request?.Headers["User-Agent"].ToStringOrNull();
        }

        public static Uri GetUri(this Microsoft.AspNetCore.Http.HttpContext context)
        {
            string host = context.Request.Scheme.EqualsI("https") && context.Request.Host.Port == 443
                          || context.Request.Scheme.EqualsI("http") && context.Request.Host.Port == 80
                ? context.Request.Host.Host
                : context.Request.Host.ToString();
            string uri = $"{context.Request.Scheme}://{host}";
            string path = context.Request.Path.ToString().TrimI("/\\ ");
            if (!string.IsNullOrWhiteSpace(path))
            {
                uri += $"/{path}";
            }

            string querystring = context.Request.QueryString.ToString().TrimI("? ");
            if (!string.IsNullOrWhiteSpace(querystring))
            {
                uri += $"?{querystring}";
            }

            return new Uri(uri);
        }

        public static Uri GetUrlReferrer(this Microsoft.AspNetCore.Http.HttpContext context)
        {
            string url = context.Request.Headers["Referer"].ToString();
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            try
            {
                return new Uri(url);
            }
            catch (UriFormatException ufe)
            {
                throw new UriFormatException($"Cannot create uri from '{url}'", ufe);
            }
        }

        public static string GetUserHostAddress(this Microsoft.AspNetCore.Http.HttpContext context)
        {
            return context.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
        }

        #region Cookies

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

        #endregion

    }
}
