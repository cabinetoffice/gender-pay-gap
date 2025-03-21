﻿namespace GenderPayGap.WebUI.Helpers
{
    /// <summary>
    /// Adds the Cache-Control no-store header to all responses to prevent caching in browsers
    /// GPG-581 We don't want to be caching html content as it may include secure info.
    /// Assets like css, js, images are still fine to cache
    /// </summary>
    public class NoHtmlCachingMiddleware
    {
        private readonly RequestDelegate _next;

        public NoHtmlCachingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.OnStarting(
                () =>
                {
                    httpContext.Response.Headers["Cache-Control"] = "no-store";
                    return Task.CompletedTask;
                });

            return this._next(httpContext);
        }
    }
}
