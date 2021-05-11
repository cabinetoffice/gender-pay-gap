using System;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Classes;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("Error")]
    public class ErrorController : BaseController
    {

        #region Constructors

        public ErrorController(
            IHttpCache cache,
            IHttpSession session,
            IDataRepository dataRepository,
            IWebTracker webTracker) : base(cache, session, dataRepository, webTracker) { }

        #endregion

        [Route("/error/")]
        [Route("/error/{errorCode?}")]
        public IActionResult Default(int errorCode = 500)
        {
            if (errorCode == 0 || !Enum.IsDefined(typeof(System.Net.HttpStatusCode), errorCode))
            {
                if (Response.StatusCode.Between(400, 599))
                {
                    errorCode = Response.StatusCode;
                }
                else
                {
                    errorCode = 500;
                }
            }

            var model = new ErrorViewModel(errorCode);

            //Get the exception which caused this error
            var errorData = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (errorData == null)
            {
                //Log non-exception events
                var statusCodeData = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
                if (statusCodeData != null)

                {
                    if (errorCode == 404 || errorCode == 405)
                    {
                        CustomLogger.Warning($"HttpStatusCode {errorCode}, Path: {statusCodeData.OriginalPath}");
                    }
                    else if (errorCode >= 400)
                    {
                        CustomLogger.Error($"HttpStatusCode {errorCode}, Path: {statusCodeData.OriginalPath}");
                    }
                }
            }

            Response.StatusCode = errorCode;
            return View("CustomError", model);
        }

        [Route("/error/service-unavailable")]
        public IActionResult ServiceUnavailable()
        {
            DateTime? dateAndTimeWhenWeExpectServiceToResume = Global.MaintenanceModeUpAgainTime;

            Response.StatusCode = 503;
            return View("ServiceUnavailable", dateAndTimeWhenWeExpectServiceToResume);
        }

    }
}
