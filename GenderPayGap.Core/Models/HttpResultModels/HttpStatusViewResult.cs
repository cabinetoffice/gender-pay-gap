using System;
using System.Net;
using System.Threading.Tasks;
using GenderPayGap.Core.Classes.Logger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GenderPayGap.Core.Models.HttpResultModels
{
    //
    // Summary:
    //     Provides a way to return an action result with a specific HTTP response status
    //     code and description.
    public class HttpStatusViewResult : ViewResult
    {
        
        //
        // Summary:
        //     Initializes a new instance of the System.Web.Mvc.HttpStatusViewResult class using
        //     a status code and status description.
        //
        // Parameters:
        //   statusCode:
        //     The status code.
        //
        //   statusDescription:
        //     The status description.
        public HttpStatusViewResult(HttpStatusCode statusCode, string statusDescription = null) : this(
            (int) statusCode,
            statusDescription)
        {
            StatusDescription = statusDescription;
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Web.Mvc.HttpStatusViewResult class using
        //     a status code and status description.
        //
        // Parameters:
        //   statusCode:
        //     The status code.
        //
        //   statusDescription:
        //     The status description.
        public HttpStatusViewResult(int statusCode, string statusDescription = null)
        {
            StatusCode = statusCode;
            StatusDescription = statusDescription;
        }

        public string StatusDescription { get; }

        public override Task ExecuteResultAsync(ActionContext actionContext)
        {
            string message;
            if (Enum.IsDefined(typeof(HttpStatusCode), StatusCode.Value))
            {
                message = $"{(HttpStatusCode)StatusCode.Value} ({StatusCode.Value}):  {StatusDescription}";
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
            
            ViewName = "Error";
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), actionContext.ModelState) {
                Model = new ErrorViewModel((int) StatusCode) // set the model
            };

            return base.ExecuteResultAsync(actionContext);
        }

    }
}
