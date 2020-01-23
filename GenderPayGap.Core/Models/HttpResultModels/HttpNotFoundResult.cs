using System.Net;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.Core.Models.HttpResultModels
{
    public class HttpNotFoundResult : HttpStatusViewResult
    {

        public HttpNotFoundResult(LogLevel logLevel = LogLevel.Warning) : this(null, logLevel) { }

        public HttpNotFoundResult(string statusDescription, LogLevel logLevel = LogLevel.Warning) : base(
            HttpStatusCode.NotFound,
            statusDescription,
            logLevel) { }

    }
}
