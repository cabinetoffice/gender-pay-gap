using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.Core.Models.HttpResultModels
{
    //
    // Summary:
    //     Provides a way to return an action result with a specific HTTP response status
    //     code and description.
    public class HttpStatusViewResult : ViewResult
    {

        private readonly LogLevel? _LogLevel;

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
        public HttpStatusViewResult(HttpStatusCode statusCode, string statusDescription = null, LogLevel? logLevel = null) : this(
            (int) statusCode,
            statusDescription,
            logLevel)
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
        public HttpStatusViewResult(int statusCode, string statusDescription = null, LogLevel? logLevel = null)
        {
            _LogLevel = logLevel;
            StatusCode = statusCode;
            StatusDescription = statusDescription;
        }

        public string StatusDescription { get; }

        public override Task ExecuteResultAsync(ActionContext actionContext)
        {
            LogLevel? logLevel = _LogLevel;

            if (logLevel == null)
            {
                if (StatusCode == 404 || StatusCode == 405)
                {
                    logLevel = LogLevel.Warning;
                }
                else if (StatusCode >= 500)
                {
                    logLevel = LogLevel.Critical;
                }
                else if (StatusCode >= 400)
                {
                    logLevel = LogLevel.Error;
                }
            }

            if (logLevel != null && logLevel != LogLevel.None)
            {
                var log = actionContext.HttpContext.RequestServices?.GetRequiredService<ILogger<HttpStatusViewResult>>();

                string message = null;
                if (Enum.IsDefined(typeof(HttpStatusCode), StatusCode.Value))
                {
                    message = $"{(HttpStatusCode) StatusCode.Value} ({StatusCode.Value}):  {StatusDescription}";
                }
                else
                {
                    message = $"HttpStatusCode ({StatusCode.Value}):  {StatusDescription}";
                }

                switch (logLevel)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                    case LogLevel.Information:
                    case LogLevel.Warning:
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        log.Log(logLevel.Value, message);
                        break;
                }
            }

            ViewName = "Error";
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), actionContext.ModelState) {
                Model = new ErrorViewModel((int) StatusCode) // set the model
            };

            return base.ExecuteResultAsync(actionContext);
        }

    }
}
