using System;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
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
    public class ReportReviewAndSubmitController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly DraftReturnService draftReturnService;

        public ReportReviewAndSubmitController(
            IDataRepository dataRepository,
            DraftReturnService draftReturnService)
        {
            this.dataRepository = dataRepository;
            this.draftReturnService = draftReturnService;
        }


        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/review-and-submit")]
        public IActionResult ReportReview(string encryptedOrganisationId, int reportingYear)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            if (!draftReturnService.DraftReturnExistsAndIsComplete(organisationId, reportingYear))
            {
                string nextPageUrl = Url.Action("ReportOverview", "ReportOverview", new { encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear });
                StatusMessageHelper.SetStatusMessage(Response, "This report is not ready to submit. Complete the remaining sections", nextPageUrl);
                return LocalRedirect(nextPageUrl);
            }

            var viewModel = new ReportReviewAndSubmitViewModel();
            PopulateReportAndSubmitViewModel(viewModel, organisationId, reportingYear);

            return View("ReportReview", viewModel);
        }

        private void PopulateReportAndSubmitViewModel(ReportReviewAndSubmitViewModel viewModel, long organisationId, int reportingYear)
        {
            viewModel.Organisation = dataRepository.Get<Organisation>(organisationId);
            viewModel.ReportingYear = reportingYear;
            viewModel.DraftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            viewModel.WillBeLateSubmission = draftReturnService.DraftReturnWouldBeLateIfSubmittedNow(viewModel.DraftReturn);
        }


        [HttpPost("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/review-and-submit")]
        [ValidateAntiForgeryToken]
        public IActionResult ReportSubmitPost(string encryptedOrganisationId, int reportingYear)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            if (!draftReturnService.DraftReturnExistsAndIsComplete(organisationId, reportingYear))
            {
                string nextPageUrl = Url.Action("ReportOverview", "ReportOverview", new { encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear });
                StatusMessageHelper.SetStatusMessage(Response, "This report is not ready to submit. Complete the remaining sections", nextPageUrl);
                return LocalRedirect(nextPageUrl);
            }

            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            if (draftReturnService.DraftReturnWouldBeLateIfSubmittedNow(draftReturn))
            {
                return RedirectToAction("LateSubmissionGet", "ReportReviewAndSubmit",
                    new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear});
            }

            return Json("This is where we would save the report (on-time submission)");
        }


        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/late-submission")]
        public IActionResult LateSubmissionGet(string encryptedOrganisationId, int reportingYear)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            if (!draftReturnService.DraftReturnExistsAndIsComplete(organisationId, reportingYear))
            {
                string nextPageUrl = Url.Action("ReportOverview", "ReportOverview", new { encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear });
                StatusMessageHelper.SetStatusMessage(Response, "This report is not ready to submit. Complete the remaining sections", nextPageUrl);
                return LocalRedirect(nextPageUrl);
            }

            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            if (!draftReturnService.DraftReturnWouldBeLateIfSubmittedNow(draftReturn))
            {
                // If (somehow) this is not a late submission, send the user back to the Review page
                return RedirectToAction("ReportReview", "ReportReviewAndSubmit",
                    new { encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear });
            }

            var viewModel = new ReportLateSubmissionViewModel();
            PopulateLateSubmissionViewModel(viewModel, organisationId, reportingYear);

            return View("LateSubmission", viewModel);
        }

        private void PopulateLateSubmissionViewModel(ReportLateSubmissionViewModel viewModel, long organisationId, int reportingYear)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            DateTime snapshotDate = organisation.SectorType.GetAccountingStartDate(reportingYear);
            DateTime deadlineDate = snapshotDate.AddYears(1).AddDays(-1);

            viewModel.Organisation = organisation;
            viewModel.ReportingYear = reportingYear;
            viewModel.DeadlineDate = deadlineDate;
        }

        [HttpPost("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/late-submission")]
        [ValidateAntiForgeryToken]
        public IActionResult LateSubmissionPost(string encryptedOrganisationId, int reportingYear, ReportLateSubmissionViewModel viewModel)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            if (!draftReturnService.DraftReturnExistsAndIsComplete(organisationId, reportingYear))
            {
                string nextPageUrl = Url.Action("ReportOverview", "ReportOverview", new { encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear });
                StatusMessageHelper.SetStatusMessage(Response, "This report is not ready to submit. Complete the remaining sections", nextPageUrl);
                return LocalRedirect(nextPageUrl);
            }

            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            if (!draftReturnService.DraftReturnWouldBeLateIfSubmittedNow(draftReturn))
            {
                // If (somehow) this is not a late submission, send the user back to the Review page
                return RedirectToAction("ReportReview", "ReportReviewAndSubmit",
                    new { encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear });
            }

            viewModel.ParseAndValidateParameters(Request, m => m.ContactFromEhrc);
            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                PopulateLateSubmissionViewModel(viewModel, organisationId, reportingYear);
                return View("LateSubmission", viewModel);
            }

            return Json("This is where we would save the report (late submission)");
        }

    }
}
