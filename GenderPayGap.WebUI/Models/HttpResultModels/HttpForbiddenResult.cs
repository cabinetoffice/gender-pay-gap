using System.Net;

namespace GenderPayGap.Core.Models.HttpResultModels
{
    public class HttpForbiddenResult : HttpStatusViewResult
    {

        public HttpForbiddenResult(string statusDescription) : base(
            HttpStatusCode.Forbidden,
            statusDescription) { }

    }
}
