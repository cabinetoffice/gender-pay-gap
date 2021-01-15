using GenderPayGap.Core;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Report
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    public class ReportCompleteFinishAndSignOutController : Controller
    {

        [HttpPost("/report-complete-finish-and-sign-out")]
        [ValidateAntiForgeryToken]
        public IActionResult ReportCompleteFinishAndSignOut()
        {
            string nextPageUrl =
                // Take the user to the "done" URL (the gov.uk survey page)
                Global.DoneUrl
                ?? // Or, if we don't have a "done URL", take them to the homepage
                Url.Action("Index", "Viewing", null, "https");

            IActionResult suggestedResult = Redirect(nextPageUrl);

            return LoginHelper.Logout(HttpContext, suggestedResult);
        }

    }
}
