using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Extensions.Http;

namespace GenderPayGap.WebUI.Helpers
{
    /// <summary>
    ///     Uses open HttpClient see
    ///     https://www.codeproject.com/Articles/1194406/Using-HttpClient-as-it-was-intended-because-you-re
    /// </summary>
    public class GoogleAnalyticsTracker : IDisposable
    {

        public static Uri BaseUri = new Uri("https://www.google-analytics.com");
        private static readonly Uri endpointUri = new Uri(BaseUri, "/collect");

        private readonly HttpClient _httpClient;

        private readonly string googleTrackingId; // UA-XXXXXXXXX-XX

        private readonly string googleVersion = "1";

        public GoogleAnalyticsTracker(HttpClient httpClient, string trackingId)
        {
            _httpClient = httpClient;
            googleTrackingId = trackingId;
        }

        private string googleClientId => Guid.NewGuid().ToString();

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
        
        public void TrackPageView(Controller controller, string pageTitle = null, string pageUrl = null)
        {
            if (string.IsNullOrWhiteSpace(pageTitle))
            {
                pageTitle = controller.ViewBag.Title;
            }

            if (string.IsNullOrWhiteSpace(pageTitle))
            {
                pageTitle = controller.RouteData.Values["action"].ToString();
            }

            if (string.IsNullOrWhiteSpace(pageUrl))
            {
                pageUrl = controller.HttpContext.Request.GetDisplayUrl();
            }
            else if (!IsUrl(pageUrl))
            {
                pageUrl = RelativeToAbsoluteUrl(pageUrl, new Uri(controller.HttpContext.Request.GetDisplayUrl()));
            }

            SendPageViewTracking(pageTitle, pageUrl);
        }
        
        private async void SendPageViewTracking(string title, string url)
        {
            if (googleTrackingId == null)
            {
                return;
            }
            
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (!IsUrl(url))
            {
                throw new ArgumentException("Url is not absolute", nameof(url));
            }

            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("v", googleVersion),
                new KeyValuePair<string, string>("tid", googleTrackingId),
                new KeyValuePair<string, string>("cid", googleClientId),
                new KeyValuePair<string, string>("t", "pageview"),
                new KeyValuePair<string, string>("dt", title),
                new KeyValuePair<string, string>("dl", url)
            };

            await _httpClient.PostAsync(endpointUri, new FormUrlEncodedContent(postData)).ConfigureAwait(false);
        }

        public static void SetupHttpClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = BaseUri;
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
            ServicePointManager.FindServicePoint(httpClient.BaseAddress).ConnectionLeaseTimeout = 60 * 1000;
        }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            //see https://developers.google.com/analytics/devguides/config/mgmt/v3/errors
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    6,
                    retryAttempt =>
                        TimeSpan.FromMilliseconds(new Random().Next(1, 1000)) + TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        private static bool IsUrl(string url)
        {
            try
            {
                if (!url.StartsWith("http:") && !url.StartsWith("https:") && !url.StartsWith("file:"))
                {
                    return false;
                }

                var uri = new Uri(url, UriKind.Absolute);
                return uri.IsAbsoluteUri;
            }
            catch
            {
                return false;
            }
        }

        private static string RelativeToAbsoluteUrl(string relativeUrl, Uri baseUrl)
        {
            if (baseUrl == null)
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            if (relativeUrl.StartsWith("http://") || relativeUrl.StartsWith("https://"))
            {
                return relativeUrl;
            }

            if (!relativeUrl.StartsWith("/"))
            {
                relativeUrl += $"/{relativeUrl}";
            }

            var baseUri = new Uri($"{baseUrl.Scheme}://{baseUrl.Authority}/");
            var absoluteUri = new Uri(baseUri, relativeUrl);
            
            return absoluteUri.ToString();
        }

    }
}
