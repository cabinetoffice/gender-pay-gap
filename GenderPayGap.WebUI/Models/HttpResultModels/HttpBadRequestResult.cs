using System.Net;

namespace GenderPayGap.Core.Models.HttpResultModels
{
    public class HttpBadRequestResult : HttpStatusViewResult
    {

        public HttpBadRequestResult() : this(null) { }

        public HttpBadRequestResult(string statusDescription) : base(
            HttpStatusCode.BadRequest,
            statusDescription) { }

    }
}
