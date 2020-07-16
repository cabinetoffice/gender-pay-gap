using System;
using System.Threading;
using GenderPayGap.Core;
using Microsoft.AspNetCore.Http;

namespace GenderPayGap.Extensions.AspNetCore
{
    public static partial class Extensions
    {

        /// <summary>
        ///     Removes null header or ensures header is set to correct value
        ///     ///
        /// </summary>
        /// <param name="context">The HttpContext to remove the header from</param>
        /// <param name="key">The key of the header name</param>
        /// <param name="value">The value which the header should be - if empty removed the header</param>
        public static void SetResponseHeader(this HttpContext context, string key, string value = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (context.Response.Headers.ContainsKey(key))
                    {
                        context.Response.Headers.Remove(key);
                    }
                }
                else if (!context.Response.Headers.ContainsKey(key))
                {
                    context.Response.Headers.Add(key, value);
                }
                else if (context.Response.Headers[key] != value)
                {
                    context.Response.Headers.Remove(key); //This is required as cannot change a key once added
                    context.Response.Headers[key] = value;
                }
            }
            catch (Exception ex)
            {
                if (context.Response.Headers.ContainsKey(key))
                {
                    throw new Exception($"Could not set header '{key}' from value '{context.Response.Headers[key]}' to '{value}' ", ex);
                }

                throw new Exception($"Could not add header '{key}' to value '{value}' ", ex);
            }
        }

        public static string GetThreadCount()
        {
            ThreadPool.GetMinThreads(out int workerMin, out int ioMin);
            ThreadPool.GetMaxThreads(out int workerMax, out int ioMax);
            ThreadPool.GetAvailableThreads(out int workerFree, out int ioFree);
            return
                $"Threads (Worker busy:{workerMax - workerFree:N0} min:{workerMin:N0} max:{workerMax:N0}, I/O busy:{ioMax - ioFree:N0} min:{ioMin:N0} max:{ioMax:N0})";
        }

        public static string SetThreadCount()
        {
            var ioMin = Global.MinIOThreads;
            var workerMin = Global.MinWorkerThreads;
            ThreadPool.SetMinThreads(workerMin, ioMin);
            return $"Min Threads Set (Work:{workerMin:N0}, I/O: {ioMin:N0})";
        }

    }
}
