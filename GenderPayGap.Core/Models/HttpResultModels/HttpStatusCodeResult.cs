using System;
using System.Net;
using System.Threading.Tasks;
using GenderPayGap.Core.Classes.Logger;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.Core.Models.HttpResultModels
{
    //
    // Summary:
    //     Provides a way to return an action result with a specific HTTP response status
    //     code and description.
    public class HttpStatusCodeResult : ContentResult
    {

        public HttpStatusCodeResult()
        {
            ContentType = "text/plain";
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Web.Mvc.HttpStatusCodeResult class using
        //     a status code and status description.
        //
        // Parameters:
        //   statusCode:
        //     The status code.
        //
        //   statusDescription:
        //     The status description.
        public HttpStatusCodeResult(int statusCode, string statusDescription = null) : this()
        {
            StatusCode = statusCode;
            Content = statusDescription;
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Web.Mvc.HttpStatusCodeResult class using
        //     a status code and status description.
        //
        // Parameters:
        //   statusCode:
        //     The status code.
        //
        //   statusDescription:
        //     The status description.
        public HttpStatusCodeResult(HttpStatusCode statusCode, string statusDescription = null) : this(
            (int) statusCode,
            statusDescription) { }

        public string StatusDescription => Content;

        public override Task ExecuteResultAsync(ActionContext actionContext)
        {
            string message;
            if (Enum.IsDefined(typeof(HttpStatusCode), StatusCode.Value))
            {
                message = $"{(HttpStatusCode) StatusCode.Value} ({StatusCode.Value}):  {StatusDescription}";
            }
            else
            {
                message = $"HttpStatusCode ({StatusCode.Value}):  {StatusDescription}";
            }
            if (StatusCode == 404 || StatusCode == 405)
            {
                CustomLogger.Warning(message);
            }
            else if (StatusCode >= 500)
            {
                CustomLogger.Fatal(message);
            }
            else if (StatusCode >= 400)
            {
                CustomLogger.Error(message);
            }

            return base.ExecuteResultAsync(actionContext);
        }

    }
}
