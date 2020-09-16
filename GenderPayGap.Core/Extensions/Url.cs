using System;
using System.IO;

namespace GenderPayGap.Extensions
{
    public static class Url
    {

        public static bool IsUrl(this string url)
        {
            try
            {
                if (!url.StartsWithI("http:") && !url.StartsWithI("https:") && !url.StartsWithI("file:"))
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

        public static bool IsRelativeUri(this string url)
        {
            return !IsAbsoluteUri(url);
        }

        public static bool IsAbsoluteUri(this string url)
        {
            return url.ContainsI("://");
        }

        public static Uri GetBaseUri(this Uri uri)
        {
            return new Uri($"{uri.Scheme}://{uri.Authority}/");
        }
        
        public static string RelativeToAbsoluteUrl(string relativeUrl, Uri baseUrl)
        {
            if (baseUrl == null)
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            if (relativeUrl.StartsWithI("http://", "https://"))
            {
                return relativeUrl;
            }

            if (!relativeUrl.StartsWith("/"))
            {
                relativeUrl += $"/{relativeUrl}";
            }

            return new Uri(GetBaseUri(baseUrl), relativeUrl).ToString();
        }

        public static string DirToUrlSeparator(string filePath)
        {
            return filePath?.Replace('\\', '/');
        }

    }
}
