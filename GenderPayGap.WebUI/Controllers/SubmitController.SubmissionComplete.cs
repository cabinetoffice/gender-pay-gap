using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.WebUI.BusinessLogic.Models.Submit;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Submission
{
    public partial class SubmitController : BaseController
    {

        [HttpGet("submission-complete")]
        public async Task<IActionResult> SubmissionComplete()
        {
            #region Check user, then retrieve model from Session

            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();

            #endregion

            if (stashedReturnViewModel == null)
            {
                stashedReturnViewModel = await submissionService.GetReturnViewModelAsync(
                    ReportingOrganisationId,
                    ReportingOrganisationStartYear.Value,
                    currentUser.UserId);
            }

            EmployerBackUrl = RequestUrl.PathAndQuery;
            ReportBackUrl = null;

            this.ClearStash();

            return View(stashedReturnViewModel);
        }

        [HttpPost("submission-complete")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmissionCompletePost(string command)
        {
            string doneUrl = Global.DoneUrl ?? Url.Action("Index", "Viewing", null, "https");
            IActionResult suggestedResult = new RedirectResult(doneUrl);

            return LoginHelper.Logout(HttpContext, suggestedResult);
        }

    }
}
