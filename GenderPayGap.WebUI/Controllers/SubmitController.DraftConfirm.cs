using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Models.Submit;
using GenderPayGap.Database;
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

            if (stashedReturnViewModel.HasReported())
            {
                await submissionService.DiscardDraftFileAsync(stashedReturnViewModel);
            }
            else
            {
                await submissionService.RollbackDraftFileAsync(stashedReturnViewModel);
            }

            return RedirectToAction(nameof(OrganisationController.ManageOrganisations), "Organisation");
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

            ConfirmPayBandsAddUpToOneHundred(postedReturnViewModel);

            ValidateBonusIntegrity(postedReturnViewModel);

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View(postedReturnViewModel.OriginatingAction, postedReturnViewModel);
            }

            await submissionService.UpdateDraftFileAsync(currentUser.UserId, postedReturnViewModel);
            await submissionService.CommitDraftFileAsync(postedReturnViewModel);

            return View("DraftComplete", postedReturnViewModel);
        }

        #endregion

    }
}
