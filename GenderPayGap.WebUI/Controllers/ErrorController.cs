using System.Net;
using GenderPayGap.Core;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("Error")]
    public class ErrorController : Controller
    {

        [Route("/error/")]
        [Route("/error/{errorCode?}")]
        public IActionResult Default(int errorCode = 500)
        {
            SetResponseStatusCodeIfValid(errorCode);

            return View("../Errors/ThereIsAProblemWithTheService");
        }

        private void SetResponseStatusCodeIfValid(int errorCode)
        {
            if (errorCode >= 400 && errorCode <= 599 && Enum.IsDefined(typeof(HttpStatusCode), errorCode))
            {
                Response.StatusCode = errorCode;
            }
            else
            {
                Response.StatusCode = 500;
            }
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
