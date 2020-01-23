using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Classes;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("Error")]
    public class ErrorController : BaseController
    {

        #region Constructors

        public ErrorController(
            ILogger<ErrorController> logger,
            IHttpCache cache,
            IHttpSession session,
            IDataRepository dataRepository,
            IWebTracker webTracker) : base(logger, cache, session, dataRepository, webTracker) { }

        #endregion

        [Route("/error/")]
        [Route("/error/{errorCode?}")]
        public IActionResult Default(int errorCode = 500)
        {
            if (errorCode == 0)
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
                        _logger.LogWarning($"HttpStatusCode {errorCode}, Path: {statusCodeData.OriginalPath}");
                    }
                    else if (errorCode >= 400)
                    {
                        _logger.LogError($"HttpStatusCode {errorCode}, Path: {statusCodeData.OriginalPath}");
                    }
                }
            }

            Response.StatusCode = errorCode;
            return View("CustomError", model);
        }

        [Route("/error/service-unavailable")]
        public IActionResult ServiceUnavailable()
        {
            var model = new ErrorViewModel(1119);
            Response.StatusCode = 503;
            return View("CustomError", model);
        }

    }
}
