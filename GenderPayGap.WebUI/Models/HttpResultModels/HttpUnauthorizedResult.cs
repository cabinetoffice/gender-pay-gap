using System.Net;

namespace GenderPayGap.Core.Models.HttpResultModels
{
    public class HttpUnauthorizedResult : HttpStatusViewResult
    {

        public HttpUnauthorizedResult() : this(null) { }

        public HttpUnauthorizedResult(string statusDescription) : base(
            HttpStatusCode.Unauthorized,
            statusDescription) { }

    }

}
