using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Models.Submit;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Submission
{
    [Authorize]
    [Route("Submit")]
    public partial class SubmitController : BaseController
    {

        public delegate bool IsPageChanged(ReturnViewModel postedReturnViewModel, ReturnViewModel stashedReturnViewModel);

        public readonly ISubmissionService submissionService;

        private readonly EmailSendingService emailSendingService;

        #region Constructors

        public SubmitController(
            IHttpCache cache,
            IHttpSession session,
            ISubmissionService submitService,
            IDataRepository dataRepository,
            IWebTracker webTracker,
            EmailSendingService emailSendingService)
            : base(cache, session, dataRepository, webTracker)
        {
            submissionService = submitService;
            this.emailSendingService = emailSendingService;
        }

        #endregion

        [HttpGet("submit/")]
        public async Task<IActionResult> Redirect()
        {
            await WebTracker.TrackPageViewAsync(this);

            return RedirectToAction("EnterCalculations");
        }

        #region private methods

        private async Task<IActionResult> ManageDraftAsync(ReturnViewModel postedReturnViewModel, IsPageChanged isPageChangedLogic)
        {
            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();
            if (stashedReturnViewModel == null)
            {
                return SessionExpiredView();
            }

            postedReturnViewModel.ReportInfo = stashedReturnViewModel.ReportInfo;

            if (!postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession)
            {
                postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession =
                    isPageChangedLogic(postedReturnViewModel, stashedReturnViewModel);
            }

            // As part of the cancellation flow, it's down to the user to confirm if the draft needs saving.
            return await PresentUserTheOptionOfSaveDraftOrIgnoreAsync(
                postedReturnViewModel,
                postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession);
        }

        private async Task<IActionResult> PresentUserTheOptionOfSaveDraftOrIgnoreAsync(ReturnViewModel postedReturnViewModel,
            bool hasADraftBeenCreated)
        {
            if (hasADraftBeenCreated)
            {
                this.StashModel(postedReturnViewModel);
                return View("DraftConfirm", postedReturnViewModel);
            }

            await submissionService.RollbackDraftFileAsync(postedReturnViewModel);
            return RedirectToAction(nameof(OrganisationController.ManageOrganisations), "Organisation");
        }

        private void ExcludeBlankFieldsFromModelState(ReturnViewModel returnViewModel)
        {
            var setOfFieldsToExclude = new HashSet<string>();

            if (!returnViewModel.DiffMeanBonusPercent.HasValue || returnViewModel.DiffMeanBonusPercent.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.DiffMeanBonusPercent));
            }

            if (!returnViewModel.DiffMeanHourlyPayPercent.HasValue || returnViewModel.DiffMeanHourlyPayPercent.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.DiffMeanHourlyPayPercent));
            }

            if (!returnViewModel.DiffMedianBonusPercent.HasValue || returnViewModel.DiffMedianBonusPercent.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.DiffMedianBonusPercent));
            }

            if (!returnViewModel.DiffMedianHourlyPercent.HasValue || returnViewModel.DiffMedianHourlyPercent.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.DiffMedianHourlyPercent));
            }

            if (!returnViewModel.FemaleLowerPayBand.HasValue || returnViewModel.FemaleLowerPayBand.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.FemaleLowerPayBand));
            }

            if (!returnViewModel.FemaleMedianBonusPayPercent.HasValue || returnViewModel.FemaleMedianBonusPayPercent.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.FemaleMedianBonusPayPercent));
            }

            if (!returnViewModel.FemaleMiddlePayBand.HasValue || returnViewModel.FemaleMiddlePayBand.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.FemaleMiddlePayBand));
            }

            if (!returnViewModel.FemaleUpperPayBand.HasValue || returnViewModel.FemaleUpperPayBand.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.FemaleUpperPayBand));
            }

            if (!returnViewModel.FemaleUpperQuartilePayBand.HasValue || returnViewModel.FemaleUpperQuartilePayBand.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.FemaleUpperQuartilePayBand));
            }

            if (returnViewModel.MaleLowerPayBand == null)
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.MaleLowerPayBand));
            }

            if (!returnViewModel.MaleMedianBonusPayPercent.HasValue || returnViewModel.MaleMedianBonusPayPercent.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.MaleMedianBonusPayPercent));
            }

            if (!returnViewModel.MaleMiddlePayBand.HasValue || returnViewModel.MaleMiddlePayBand.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.MaleMiddlePayBand));
            }

            if (!returnViewModel.MaleUpperPayBand.HasValue || returnViewModel.MaleUpperPayBand.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.MaleUpperPayBand));
            }

            if (!returnViewModel.MaleUpperQuartilePayBand.HasValue || returnViewModel.MaleUpperQuartilePayBand.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.MaleUpperQuartilePayBand));
            }

            if (string.IsNullOrWhiteSpace(returnViewModel.FirstName)
                && string.IsNullOrWhiteSpace(returnViewModel.LastName)
                && string.IsNullOrWhiteSpace(returnViewModel.JobTitle))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.JobTitle));
                setOfFieldsToExclude.Add(nameof(returnViewModel.FirstName));
                setOfFieldsToExclude.Add(nameof(returnViewModel.LastName));
            }

            if (returnViewModel.LateReason.IsNullOrWhiteSpace())
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.LateReason));
            }

            if (returnViewModel.EHRCResponse.IsNullOrWhiteSpace())
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.EHRCResponse));
            }

            ModelState.Exclude(setOfFieldsToExclude.ToArray());
        }

        private async Task<ReturnViewModel> LoadReturnViewModelFromDBorFromDraftFileAsync(ReturnViewModel stashedReturnViewModel,
            long currentUserId)
        {
            if (stashedReturnViewModel == null)
            {
                /* info is not stashed, so load the return info from somewhere (db or draft-azure) */
                stashedReturnViewModel = await submissionService.GetReturnViewModelAsync(
                    ReportingOrganisationId,
                    ReportingOrganisationStartYear.Value,
                    currentUserId);
            }

            return stashedReturnViewModel;
        }

        #endregion

    }
}
