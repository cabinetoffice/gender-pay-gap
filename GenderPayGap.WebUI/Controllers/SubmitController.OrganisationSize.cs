using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Models.Submit;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Submission
{
    public partial class SubmitController : BaseController
    {

        #region private methods

        private static bool IsOrganisationSizeModified(ReturnViewModel postedReturnViewModel, ReturnViewModel stashedReturnViewModel)
        {
            return postedReturnViewModel.OrganisationSize != stashedReturnViewModel.OrganisationSize;
        }

        #endregion

        #region public methods

        [HttpGet("organisation-size")]
        public async Task<IActionResult> OrganisationSize(string returnUrl = null)
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
                return SessionExpiredView();
            }

            stashedReturnViewModel = await LoadReturnViewModelFromDBorFromDraftFileAsync(stashedReturnViewModel, currentUser.UserId);

            if (!stashedReturnViewModel.ReportInfo.Draft.IsUserAllowedAccess)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CustomError", new ErrorViewModel(3040));
            }

            stashedReturnViewModel.ReturnUrl = returnUrl;

            return View("OrganisationSize", stashedReturnViewModel);
        }

        [HttpPost("organisation-size")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrganisationSize(ReturnViewModel postedReturnViewModel, string returnUrl = null)
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
                return SessionExpiredView();
            }

            postedReturnViewModel.ReportInfo = stashedReturnViewModel.ReportInfo;

            ModelState.Include(nameof(postedReturnViewModel.OrganisationSize));

            #region Keep draft file locked to this user

            await submissionService.KeepDraftFileLockedToUserAsync(postedReturnViewModel, CurrentUser.UserId);

            if (!postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession)
            {
                postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession =
                    IsOrganisationSizeModified(postedReturnViewModel, stashedReturnViewModel);
            }

            if (!stashedReturnViewModel.ReportInfo.Draft.IsUserAllowedAccess)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CustomError", new ErrorViewModel(3040));
            }

            #endregion

            this.StashModel(postedReturnViewModel);

            return RedirectToAction(returnUrl.EqualsI("CheckData") ? "CheckData" : "EmployerWebsite");
        }

        [HttpPost("cancel-organisation-size")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrganisationSize(ReturnViewModel postedReturnViewModel)
        {
            postedReturnViewModel.OriginatingAction = "OrganisationSize";
            return await ManageDraftAsync(postedReturnViewModel, IsOrganisationSizeModified);
        }

        #endregion

    }
}
