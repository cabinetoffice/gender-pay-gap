using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.WebUI.BusinessLogic.Classes;
using GenderPayGap.WebUI.BusinessLogic.Models.Submit;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Submit;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.Controllers.Submission
{
    public partial class SubmitController : BaseController
    {

        #region private methods

        private async Task TryToReloadDraftContent(ReturnViewModel stashedReturnViewModel)
        {
            Draft availableDraft = await submissionService.GetDraftIfAvailableAsync(
                stashedReturnViewModel.OrganisationId,
                stashedReturnViewModel.AccountingDate.Year);

            if (availableDraft != null && availableDraft.HasContent())
            {
                stashedReturnViewModel.ReportInfo.Draft.ReturnViewModelContent = availableDraft.ReturnViewModelContent;
            }
        }

        private string GetReportLink(Return postedReturn)
        {
            return Url.Action(
                "Report",
                "Viewing",
                new {employerIdentifier = postedReturn.Organisation.GetEncryptedId(), year = postedReturn.AccountingDate.Year},
                "https");
        }

        private string GetSubmittedOrUpdated(Return postedReturn)
        {
            List<Return> otherReturns =
                postedReturn.Organisation.Returns
                    .Except(new[] {postedReturn})
                    .Where(r => r.AccountingDate == postedReturn.AccountingDate)
                    .ToList();

            return otherReturns.Count > 0 ? "updated" : "submitted";
        }

        #endregion

        #region public methods

        [HttpGet("check-data")]
        public async Task<IActionResult> CheckData()
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

            if (stashedReturnViewModel.ReportInfo.Draft != null && !stashedReturnViewModel.ReportInfo.Draft.IsUserAllowedAccess)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CustomError", new ErrorViewModel(3040));
            }


            if (!stashedReturnViewModel.HasDraftWithContent())
            {
                await TryToReloadDraftContent(stashedReturnViewModel);
            }

            if (stashedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession
                || stashedReturnViewModel.HasDraftWithContent())
            {
                Return databaseReturn = await submissionService.GetSubmissionByIdAsync(stashedReturnViewModel.ReturnId);

                if (databaseReturn != null)
                {
                    Return stashedReturn = submissionService.CreateDraftSubmissionFromViewModel(stashedReturnViewModel);
                    SubmissionChangeSummary changeSummary = submissionService.GetSubmissionChangeSummary(stashedReturn, databaseReturn);
                    stashedReturnViewModel.IsDifferentFromDatabase = changeSummary.HasChanged;
                    stashedReturnViewModel.ShouldProvideLateReason = changeSummary.ShouldProvideLateReason;
                }
                else
                {
                    // We have some draft info and no DB record, therefore is definitely different
                    stashedReturnViewModel.IsDifferentFromDatabase = true;
                    // Recalculate to know if they're submitting late. This is because it is possible that a draft was created BEFORE the cut-off date ("should provide late reason" would have been marked as 'false') but are completing the submission process AFTER which is when we need them to provide a late reason and the flag is expected to be 'true'.
                    stashedReturnViewModel.ShouldProvideLateReason = submissionService.IsHistoricSnapshotYear(
                        stashedReturnViewModel.OrganisationSector,
                        ReportingOrganisationStartYear.Value);
                }
            }

            if (!submissionService.IsValidSnapshotYear(ReportingOrganisationStartYear.Value))
            {
                return new HttpBadRequestResult($"Invalid snapshot year {ReportingOrganisationStartYear.Value}");
            }

            // Don't ask for late reason if the reporting year has been excluded from late flag enforcement (eg. 2019/20 due to COVID-19)
            if (Global.ReportingStartYearsToExcludeFromLateFlagEnforcement.Contains(stashedReturnViewModel.AccountingDate.Year))
            {
                stashedReturnViewModel.ShouldProvideLateReason = false;
            }

            this.StashModel(stashedReturnViewModel);
            return View("CheckData", stashedReturnViewModel);
        }

        [HttpPost("check-data")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckData(ReturnViewModel postedReturnViewModel)
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

            postedReturnViewModel.ReportInfo.Draft = stashedReturnViewModel.ReportInfo.Draft;

            if (postedReturnViewModel.OrganisationSector == OrganisationSectors.Public)
            {
                ModelState.Exclude(
                    nameof(postedReturnViewModel.FirstName),
                    nameof(postedReturnViewModel.LastName),
                    nameof(postedReturnViewModel.JobTitle));
            }

            Return postedReturn = submissionService.CreateDraftSubmissionFromViewModel(postedReturnViewModel);

            SubmissionChangeSummary changeSummary = null;
            Return databaseReturn = await submissionService.GetSubmissionByIdAsync(postedReturnViewModel.ReturnId);
            if (databaseReturn != null)
            {
                changeSummary = submissionService.GetSubmissionChangeSummary(postedReturn, databaseReturn);

                if (!changeSummary.HasChanged)
                {
                    return new HttpBadRequestResult("Submission has no changes");
                }

                if (!changeSummary.FiguresChanged)
                {
                    // If the figure have not changed
                    //   e.g. if the only change was to the URL / person reporting
                    // Then don't apply a NEW late flag
                    // But DO continue to apply an EXISTING late flag
                    // So, in summary, "If the figure have not changed, copy the old late flag"
                    postedReturn.IsLateSubmission = databaseReturn.IsLateSubmission;
                }

                postedReturn.Modifications = changeSummary.Modifications;

                databaseReturn.SetStatus(ReturnStatuses.Retired, OriginalUser == null ? currentUser.UserId : OriginalUser.UserId);
            }

            if (databaseReturn == null || !changeSummary.ShouldProvideLateReason)
            {
                ModelState.Remove(nameof(postedReturnViewModel.LateReason));
                ModelState.Remove(nameof(postedReturnViewModel.EHRCResponse));
            }

            // Don't mark submission as late if the reporting year has been excluded from late flag enforcement (eg. 2019/20 due to COVID-19)
            if (Global.ReportingStartYearsToExcludeFromLateFlagEnforcement.Contains(stashedReturnViewModel.AccountingDate.Year))
            {
                ModelState.Remove(nameof(postedReturnViewModel.LateReason));
                ModelState.Remove(nameof(postedReturnViewModel.EHRCResponse));
                postedReturn.IsLateSubmission = false;
            }

            ModelState.Remove("ReportInfo.Draft");

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CheckData", postedReturnViewModel);
            }

            if (databaseReturn == null || databaseReturn.Status == ReturnStatuses.Retired)
            {
                DataRepository.Insert(postedReturn);
            }

            postedReturn.SetStatus(ReturnStatuses.Submitted, OriginalUser?.UserId ?? currentUser.UserId);

            Organisation organisationFromDatabase = await DataRepository.GetAll<Organisation>()
                .FirstOrDefaultAsync(o => o.OrganisationId == postedReturnViewModel.OrganisationId);

            organisationFromDatabase.Returns.Add(postedReturn);

            await DataRepository.SaveChangesAsync();

            //This is required for the submission complete page
            postedReturnViewModel.EncryptedOrganisationId = postedReturn.Organisation.GetEncryptedId();
            this.StashModel(postedReturnViewModel);

            if (Global.EnableSubmitAlerts
                && postedReturn.Organisation.Returns.Count(r => r.AccountingDate == postedReturn.AccountingDate) == 1)
            {
                emailSendingService.SendGeoFirstTimeDataSubmissionEmail(
                    postedReturn.AccountingDate.Year.ToString(),
                    postedReturn.Organisation.OrganisationName,
                    postedReturn.StatusDate.ToShortDateString(),
                    Url.Action(
                        "Report",
                        "Viewing",
                        new {employerIdentifier = postedReturnViewModel.EncryptedOrganisationId, year = postedReturn.AccountingDate.Year},
                        "https"));
            }

            EmailSendingServiceHelpers.SendSuccessfulSubmissionEmailToRegisteredUsers(
                postedReturn,
                GetReportLink(postedReturn),
                GetSubmittedOrUpdated(postedReturn),
                emailSendingService);

            await submissionService.DiscardDraftFileAsync(postedReturnViewModel);

            return RedirectToAction("SubmissionComplete");
        }

        [HttpPost("cancel-check-data")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelCheckData(ReturnViewModel postedReturnViewModel)
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

            postedReturnViewModel.ReportInfo.Draft = stashedReturnViewModel.ReportInfo.Draft;

            postedReturnViewModel.OriginatingAction = "CheckData";
            bool hasDraftChanged = postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession;
            return await PresentUserTheOptionOfSaveDraftOrIgnoreAsync(postedReturnViewModel, hasDraftChanged);
        }

        #endregion

    }
}
