using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace GovUkDesignSystem.Helpers
{
    internal static class HttpRequestParameterHelper
    {

        internal static StringValues GetRequestParameter(HttpRequest httpRequest, string parameterName)
        {
            if (httpRequest.HasFormContentType)
            {
                return httpRequest.Form[parameterName];
            }
            else
            {
                return httpRequest.Query[parameterName];
            }
        }

    }
}
