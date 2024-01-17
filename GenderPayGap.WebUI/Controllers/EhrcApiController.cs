using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("ehrc-api")]
    public class EhrcApiController : Controller
    {

        private readonly IDataRepository dataRepository;

        public EhrcApiController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string token = GetQueryStringParameter(filterContext, "token");

            if (token != Global.EhrcApiToken)
            {
                filterContext.Result = Forbid();
            }
        }

        private string GetQueryStringParameter(ActionExecutingContext filterContext, string parameterName)
        {
            if (filterContext.HttpContext.Request.Query.TryGetValue(parameterName, out StringValues parameterStringValues) &&
                parameterStringValues.Count == 1)
            {
                return parameterStringValues[0];
            }

            return null;
        }

        [HttpGet("processed/all-organisations/{year}")]
        public IActionResult EhrcAllOrganisationsForYear_AdminPage(int year)
        {
            return AdminDownloadsController.GenerateEhrcAllOrganisationsForYearFile(dataRepository, year);
        }

        [HttpGet("processed/organisations-without-submitted-returns-with-login-data/{year}")]
        public FileContentResult DownloadOrganisationsWithNoSubmittedReturnsAndRecentLoginData(int year)
        {
            return AdminDownloadsController.GenerateOrganisationsWithNoSubmittedReturnsForYear(dataRepository, year, true);
        }

        [HttpGet("processed/full-submission-history/{year}")]
        public FileContentResult DownloadFullSubmissionHistoryForYear(int year)
        {
            return AdminDownloadsController.GenerateFullSubmissionHistoryForYear(dataRepository, year);
        }

    }
}
