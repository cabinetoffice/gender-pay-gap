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
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Report
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("account/organisations")]
    public class NewLateSubmissionController: Controller
    {
        
        private readonly IDataRepository dataRepository;
        private readonly DraftReturnService draftReturnService;
        private readonly ReturnService returnService;

        public NewLateSubmissionController(
            IDataRepository dataRepository,
            DraftReturnService draftReturnService,
            ReturnService returnService)
        {
            this.dataRepository = dataRepository;
            this.draftReturnService = draftReturnService;
            this.returnService = returnService;
        }

        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/new-late-submission-warning")]
        public IActionResult NewLateSubmissionWarningGet(string encryptedOrganisationId, int reportingYear)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);
            
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            var viewModel = new NewLateSubmissionWarningViewModel();
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);

            viewModel.Organisation = organisation;
            viewModel.ReportingYear = reportingYear;

            return View("~/Views/ReportReviewAndSubmit/NewLateSubmissionWarning.cshtml", viewModel);
        }
        
        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/new-late-submission")]
        public IActionResult NewLateSubmissionReasonGet(string encryptedOrganisationId, int reportingYear)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            if (!draftReturnService.DraftReturnExistsAndRequiredFieldsAreComplete(organisationId, reportingYear))
            {
                return RedirectToReportOverviewPage(encryptedOrganisationId, reportingYear, "This report is not ready to submit. Complete the remaining sections");
            }

            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            if (!draftReturnService.DraftReturnWouldBeNewlyLateIfSubmittedNow(draftReturn))
            {
                // If this is not a late submission, send the user back to the Overview page
                return RedirectToAction("NewReportOverview", "NewReportOverview",
                    new { encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear });
            }

            var viewModel = new NewLateSubmissionReasonViewModel();
            PopulateLateSubmissionViewModel(viewModel, organisationId, reportingYear);

            return View("~/Views/ReportReviewAndSubmit/NewLateSubmissionReason.cshtml", viewModel);
        }

        private void PopulateLateSubmissionViewModel(NewLateSubmissionReasonViewModel viewModel, long organisationId, int reportingYear)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            
            DateTime snapshotDate = organisation.SectorType.GetAccountingStartDate(reportingYear);
            DateTime deadlineDate = ReportingYearsHelper.GetDeadlineForAccountingDate(snapshotDate);

            Return submittedReturn = organisation.GetReturn(reportingYear);
            bool isEditingSubmittedReturn = submittedReturn != null;
            
            viewModel.Organisation = organisation;
            viewModel.ReportingYear = reportingYear;
            viewModel.DeadlineDate = deadlineDate;
            viewModel.IsEditingSubmittedReturn = isEditingSubmittedReturn;
        }

        [HttpPost("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/new-late-submission")]
        [ValidateAntiForgeryToken]
        public IActionResult NewLateSubmissionReasonPost(string encryptedOrganisationId, int reportingYear, NewLateSubmissionReasonViewModel viewModel)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            if (!draftReturnService.DraftReturnExistsAndRequiredFieldsAreComplete(organisationId, reportingYear))
            {
                return RedirectToReportOverviewPage(encryptedOrganisationId, reportingYear, "This report is not ready to submit. Complete the remaining sections");
            }

            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            if (!draftReturnService.DraftReturnWouldBeNewlyLateIfSubmittedNow(draftReturn))
            {
                // If this is not a late submission, send the user back to the Overview page
                return RedirectToAction("NewReportOverview", "NewReportOverview",
                    new { encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear });
            }

            viewModel.ParseAndValidateParameters(Request, m => m.ReceivedLetterFromEhrc);
            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                PopulateLateSubmissionViewModel(viewModel, organisationId, reportingYear);
                return View("~/Views/ReportReviewAndSubmit/NewLateSubmissionReason.cshtml", viewModel);
            }

            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            bool receivedLetterFromEhrc = viewModel.ReceivedLetterFromEhrc == NewReportLateSubmissionReceivedLetterFromEhrc.Yes;
            Return newReturn = returnService.CreateAndSaveLateReturnFromDraftReturn(draftReturn, user, Url, viewModel.Reason, receivedLetterFromEhrc);

            return RedirectToAction("NewReportConfirmation", "NewReportConfirmation",
                new
                {
                    encryptedOrganisationId = encryptedOrganisationId,
                    reportingYear = reportingYear,
                    confirmationId = Encryption.EncryptQuerystring(newReturn.ReturnId.ToString())
                });
        }
        
        private IActionResult RedirectToReportOverviewPage(string encryptedOrganisationId, int reportingYear, string message)
        {
            string nextPageUrl = Url.Action("NewReportOverview", "NewReportOverview", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear});
            StatusMessageHelper.SetStatusMessage(Response, message, nextPageUrl);
            return LocalRedirect(nextPageUrl);
        }

    }
}
