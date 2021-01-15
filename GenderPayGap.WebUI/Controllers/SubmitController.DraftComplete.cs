using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.WebUI.BusinessLogic.Models.Submit;
using GenderPayGap.WebUI.Classes;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Submission
{
    public partial class SubmitController : BaseController
    {

        #region public methods

        [HttpGet("draft-complete")]
        public async Task<IActionResult> DraftComplete()
        {
            #region Check user, then retrieve model from Session

            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();

            #endregion

            stashedReturnViewModel = await LoadReturnViewModelFromDBorFromDraftFileAsync(stashedReturnViewModel, currentUser.UserId);

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("DraftComplete", stashedReturnViewModel);
            }

            submissionService.UpdateAndCommitDraftFile(currentUser.UserId, stashedReturnViewModel);

            return View("DraftComplete", stashedReturnViewModel);
        }

        [HttpPost("draft-complete")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult DraftCompletePost(string command)
        {
            string doneUrl = Global.DoneUrl ?? Url.Action("Index", "Viewing", null, "https");

            return Redirect(doneUrl);
        }

        #endregion

    }
}
