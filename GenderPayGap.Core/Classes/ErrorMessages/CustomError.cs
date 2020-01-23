using System;
using System.Net;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Classes.ErrorMessages
{
    [Serializable]
    public class CustomError
    {

        public CustomError(int code, string description)
        {
            Code = code;
            Description = description;
        }

        public CustomError(HttpStatusCode httpStatusCode, string description)
            : this(httpStatusCode.ToInt32(), description) { }

        public int Code { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return Description;
        }

        public HttpStatusViewResult ToHttpStatusViewResult()
        {
            HttpStatusCode codeConvertedToStatus = Enum.IsDefined(typeof(HttpStatusCode), Code)
                ? (HttpStatusCode) Code
                : HttpStatusCode.NotFound;

            return new HttpStatusViewResult((int) codeConvertedToStatus, Description);
        }

        public HttpException ToHttpException()
        {
            HttpStatusCode codeConvertedToStatus = Enum.IsDefined(typeof(HttpStatusCode), Code)
                ? (HttpStatusCode) Code
                : HttpStatusCode.NotFound;

            return new HttpException(codeConvertedToStatus, Description);
        }

    }
}
