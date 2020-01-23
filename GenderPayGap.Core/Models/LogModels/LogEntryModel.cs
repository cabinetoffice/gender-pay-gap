using System;
using System.Web;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Models
{
    [Serializable]
    public class LogEntryModel
    {

        public DateTime Date { get; set; } = VirtualDateTime.Now;
        public string Machine { get; set; } = $"Machine:{Environment.MachineName}";
        public string HttpMethod { get; set; } = HttpContext.Current?.Request?.Method;
        public string WebPath { get; set; } = HttpContext.Current?.GetUri()?.PathAndQuery;
        public string RemoteIP { get; set; } = HttpContext.Current?.GetUserHostAddress();
        public string Source { get; set; } = AppDomain.CurrentDomain.FriendlyName;
        public string Message { get; set; }
        public string Details { get; set; }
        public string Stacktrace { get; set; }

    }
}
