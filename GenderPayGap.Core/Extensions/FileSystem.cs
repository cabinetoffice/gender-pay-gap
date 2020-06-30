using System;
using System.IO;

namespace GenderPayGap.Extensions
{
    public static class FileSystem
    {

        /// Expands a condensed path relative to the application path (or basePath) up to a full path
        public static string ExpandLocalPath(string path, string basePath = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(basePath))
            {
                basePath = AppDomain.CurrentDomain.BaseDirectory;
            }

            path = path.Replace(@"/", @"\");
            path = path.Replace(@"~\", @".\");
            path = path.Replace(@"\\", @"\");

            if (path.StartsWith(@".\") || path.StartsWith(@"..\"))
            {
                var uri = new Uri(Path.Combine(basePath, path));
                return Path.GetFullPath(uri.LocalPath);
            }

            while (path.StartsWithAny('\\', '/'))
            {
                path = path.Substring(1);
            }

            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(basePath, path);
            }

            return path;
        }

    }
}
