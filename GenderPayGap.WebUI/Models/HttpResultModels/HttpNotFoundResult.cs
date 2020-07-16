using System.Net;

namespace GenderPayGap.Core.Models.HttpResultModels
{
    public class HttpNotFoundResult : HttpStatusViewResult
    {

        public HttpNotFoundResult() : this(null) { }

        public HttpNotFoundResult(string statusDescription) : base(
            HttpStatusCode.NotFound,
            statusDescription) { }

    }
}
