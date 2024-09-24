using System;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Report;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Report
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("account/organisations")]
    public class LateSubmissionController: Controller
    {
        
        private readonly IDataRepository dataRepository;
        private readonly DraftReturnService draftReturnService;
        private readonly ReturnService returnService;

        public LateSubmissionController(
            IDataRepository dataRepository,
            DraftReturnService draftReturnService,
            ReturnService returnService)
        {
            this.dataRepository = dataRepository;
            this.draftReturnService = draftReturnService;
            this.returnService = returnService;
        }

        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/late-submission-warning")]
        public IActionResult LateSubmissionWarningGet(string encryptedOrganisationId, int reportingYear)
        {
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear, organisationId, dataRepository);

            var viewModel = new LateSubmissionWarningViewModel();
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);

            viewModel.Organisation = organisation;
            viewModel.ReportingYear = reportingYear;

            return View("LateSubmissionWarning", viewModel);
        }
        
        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/late-submission")]
        public IActionResult LateSubmissionReasonGet(string encryptedOrganisationId, int reportingYear)
        {
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear, organisationId, dataRepository);

            if (!draftReturnService.DraftReturnExistsAndRequiredFieldsAreComplete(organisationId, reportingYear))
            {
                return RedirectToReportOverviewPage(encryptedOrganisationId, reportingYear, "This report is not ready to submit. Complete the remaining sections");
            }

            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            if (!draftReturnService.DraftReturnWouldBeNewlyLateIfSubmittedNow(draftReturn))
            {
                // If this is not a late submission, send the user back to the Overview page
                return RedirectToAction("ReportOverview", "ReportOverview",
                    new { encryptedOrganisationId, reportingYear });
            }

            var viewModel = new LateSubmissionReasonViewModel();
            PopulateLateSubmissionViewModel(viewModel, organisationId, reportingYear);

            return View("LateSubmissionReason", viewModel);
        }

        [HttpPost("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/late-submission")]
        [ValidateAntiForgeryToken]
        public IActionResult LateSubmissionReasonPost(string encryptedOrganisationId, int reportingYear, LateSubmissionReasonViewModel viewModel)
        {
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear, organisationId, dataRepository);

            if (!draftReturnService.DraftReturnExistsAndRequiredFieldsAreComplete(organisationId, reportingYear))
            {
                return RedirectToReportOverviewPage(encryptedOrganisationId, reportingYear, "This report is not ready to submit. Complete the remaining sections");
            }

            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            if (!draftReturnService.DraftReturnWouldBeNewlyLateIfSubmittedNow(draftReturn))
            {
                // If this is not a late submission, send the user back to the Overview page
                return  RedirectToReportOverviewPage(encryptedOrganisationId, reportingYear);
            }

            if (!ModelState.IsValid)
            {
                PopulateLateSubmissionViewModel(viewModel, organisationId, reportingYear);
                return View("LateSubmissionReason", viewModel);
            }

            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            bool receivedLetterFromEhrc = viewModel.ReceivedLetterFromEhrc == ReportLateSubmissionReceivedLetterFromEhrc.Yes;
            Return newReturn = returnService.CreateAndSaveLateReturnFromDraftReturn(draftReturn, user, Url, viewModel.Reason, receivedLetterFromEhrc);

            return RedirectToAction("ReportConfirmation", "ReportConfirmation",
                new
                {
                    encryptedOrganisationId,
                    reportingYear,
                    confirmationId = Encryption.EncryptQuerystring(newReturn.ReturnId.ToString())
                });
        }
        
        private void PopulateLateSubmissionViewModel(LateSubmissionReasonViewModel viewModel, long organisationId, int reportingYear)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            
            DateTime snapshotDate = organisation.SectorType.GetAccountingStartDate(reportingYear);
            DateTime deadlineDate = ReportingYearsHelper.GetDeadlineForAccountingDate(snapshotDate);

            viewModel.Organisation = organisation;
            viewModel.ReportingYear = reportingYear;
            viewModel.DeadlineDate = deadlineDate;
            viewModel.IsEditingSubmittedReturn = organisation.HasSubmittedReturn(reportingYear);
        }
        
        private IActionResult RedirectToReportOverviewPage(string encryptedOrganisationId, int reportingYear, string message = null)
        {
            string nextPageUrl = Url.Action("ReportOverview", "ReportOverview", new { encryptedOrganisationId, reportingYear});

            if (message != null)
            {
                StatusMessageHelper.SetStatusMessage(Response, message, nextPageUrl);
            }
            return LocalRedirect(nextPageUrl);
        }

    }
}
