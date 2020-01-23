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

        //
        // Summary:
        //     Returns the directory information for the specified path string.
        //
        // Parameters:
        //   path:
        //     The path of a Uri, file or directory.
        //
        // Returns:
        //     Directory information for path, or null if path denotes a root directory or is
        //     null. Returns System.String.Empty if path does not contain directory information.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     The path parameter contains invalid characters, is empty, or contains only white
        //     spaces.
        //
        //   T:System.IO.PathTooLongException:
        //     In the [.NET for Windows Store apps](http://go.microsoft.com/fwlink/?LinkID=247912)
        //     or the [Portable Class Library](~/docs/standard/cross-platform/cross-platform-development-with-the-portable-class-library.md),
        //     catch the base class exception, System.IO.IOException, instead. The path parameter
        //     is longer than the system-defined maximum length.
        public static string GetDirectoryName(string path)
        {
            return DirToUrlSeparator(Path.GetDirectoryName(path));
        }

        //
        // Summary:
        //     Combines an array of strings into a path.
        //
        // Parameters:
        //   paths:
        //     An array of parts of the path.
        //
        // Returns:
        //     The combined paths.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     One of the strings in the array contains one or more of the invalid characters
        //     defined in System.IO.Path.GetInvalidPathChars.
        //
        //   T:System.ArgumentNullException:
        //     One of the strings in the array is null.
        public static string Combine(params string[] paths)
        {
            return DirToUrlSeparator(Path.Combine(paths));
        }


        public static string DirToUrlSeparator(string filePath)
        {
            return filePath?.Replace('\\', '/');
        }

        public static string UrlToDirSeparator(string filePath)
        {
            return filePath?.Replace('/', '\\');
        }

    }
}
