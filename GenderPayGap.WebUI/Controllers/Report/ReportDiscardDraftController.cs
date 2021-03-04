using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Report;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Report
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("account/organisations")]
    public class ReportDiscardDraftController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly DraftReturnService draftReturnService;

        public ReportDiscardDraftController(
            IDataRepository dataRepository,
            DraftReturnService draftReturnService)
        {
            this.dataRepository = dataRepository;
            this.draftReturnService = draftReturnService;
        }


        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/discard-draft")]
        public IActionResult ReportDiscardDraftGet(string encryptedOrganisationId, int reportingYear)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            bool draftReturnExists = draftReturn != null;

            if (!draftReturnExists)
            {
                string nextPageUrl = Url.Action("ReportOverview", "ReportOverview", new { encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear });
                StatusMessageHelper.SetStatusMessage(Response, "No draft to discard", nextPageUrl);
                return LocalRedirect(nextPageUrl);
            }

            var viewModel = new ReportDiscardDraftViewModel();
            PopulateViewModel(viewModel, organisationId, reportingYear);

            return View("ReportDiscardDraft", viewModel);
        }

        private void PopulateViewModel(ReportDiscardDraftViewModel viewModel, long organisationId, int reportingYear)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);

            viewModel.Organisation = organisation;
            viewModel.ReportingYear = reportingYear;

            Return submittedReturn = organisation.GetReturn(reportingYear);
            bool isEditingSubmittedReturn = submittedReturn != null;
            viewModel.IsEditingSubmittedReturn = isEditingSubmittedReturn;
        }


        [HttpPost("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/discard-draft")]
        [ValidateAntiForgeryToken]
        public IActionResult ReportDiscardDraftPost(string encryptedOrganisationId, int reportingYear)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            bool draftReturnExists = draftReturn != null;
            if (!draftReturnExists)
            {
                string nextPageNoDraftUrl = Url.Action("ReportOverview", "ReportOverview", new { encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear });
                StatusMessageHelper.SetStatusMessage(Response, "No draft to discard", nextPageNoDraftUrl);
                return LocalRedirect(nextPageNoDraftUrl);
            }

            dataRepository.Delete(draftReturn);
            dataRepository.SaveChanges();

            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            Return submittedReturn = organisation.GetReturn(reportingYear);
            bool isEditingSubmittedReturn = submittedReturn != null;

            string yourChangesOrYourDraftReport = isEditingSubmittedReturn ? "your changes" : "your draft report";

            string nextPageUrl = Url.Action("ReportOverview", "ReportOverview", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear});
            StatusMessageHelper.SetStatusMessage(Response, $"Discarded {yourChangesOrYourDraftReport}", nextPageUrl);
            return LocalRedirect(nextPageUrl);
        }

    }
}
