using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace HoldingPage.Controllers
{
    [Route("Error")]
    public class ErrorController : Controller
    {

        [Route("/error/")]
        [Route("/error/{errorCode?}")]
        public IActionResult Default(int errorCode = 500)
        {
            SetResponseStatusCodeIfValid(errorCode);

            return View("../Homepage/Index");
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

    }
}
