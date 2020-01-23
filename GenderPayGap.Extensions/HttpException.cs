using System;
using System.Net;

namespace GenderPayGap.Extensions
{
    public class HttpException : Exception
    {

        public readonly int StatusCode;

        public HttpException(int statusCode, string message = null) : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpException(HttpStatusCode statusCode, string message = null) : base(message)
        {
            StatusCode = (int) statusCode;
        }

    }
}
