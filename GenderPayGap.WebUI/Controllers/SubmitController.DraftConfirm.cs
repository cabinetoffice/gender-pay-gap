using System.Threading.Tasks;
using GenderPayGap.Database;
using GenderPayGap.WebUI.BusinessLogic.Models.Submit;
using GenderPayGap.WebUI.Classes;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Submission
{
    public partial class SubmitController
    {

        #region private methods

        [HttpPost("exit-without-saving")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExitWithoutSaving()
        {
            #region Check user, then retrieve model from Session

            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();

            #endregion

            await submissionService.DiscardDraftFileAsync(stashedReturnViewModel);

            return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
        }

        [HttpPost("save-draft")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDraftAsync(ReturnViewModel postedReturnViewModel)
        {
            #region Check user, then retrieve model from Session

            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();

            #endregion

            postedReturnViewModel.ReportInfo = stashedReturnViewModel.ReportInfo;

            ExcludeBlankFieldsFromModelState(postedReturnViewModel);
            
            ValidatePayBands(postedReturnViewModel);

            ValidateBonusIntegrity(postedReturnViewModel);

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View(postedReturnViewModel.OriginatingAction, postedReturnViewModel);
            }

            submissionService.UpdateAndCommitDraftFile(currentUser.UserId, postedReturnViewModel);

            return View("DraftComplete", postedReturnViewModel);
        }

        #endregion

    }
}
