using System.Net;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.Core.Models.HttpResultModels
{
    public class HttpBadRequestResult : HttpStatusViewResult
    {

        public HttpBadRequestResult(LogLevel logLevel = LogLevel.Warning) : this(null, logLevel) { }

        public HttpBadRequestResult(string statusDescription, LogLevel logLevel = LogLevel.Warning) : base(
            HttpStatusCode.BadRequest,
            statusDescription,
            logLevel) { }

    }
}
