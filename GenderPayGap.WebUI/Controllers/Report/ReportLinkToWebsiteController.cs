using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Report;
using GenderPayGap.WebUI.Services;
using GovUkDesignSystem;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Report
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("account/organisations")]
    public class ReportLinkToWebsiteController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly DraftReturnService draftReturnService;

        public ReportLinkToWebsiteController(
            IDataRepository dataRepository,
            DraftReturnService draftReturnService)
        {
            this.dataRepository = dataRepository;
            this.draftReturnService = draftReturnService;
        }


        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/link-to-organisation-website")]
        public IActionResult ReportLinkToWebsiteGet(string encryptedOrganisationId, int reportingYear)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            var viewModel = new ReportLinkToWebsiteViewModel();
            PopulateViewModelWithOrganisationAndReportingYear(viewModel, organisationId, reportingYear);
            SetValuesFromDraftReturnOrSubmittedReturn(viewModel, organisationId, reportingYear);

            return View("ReportLinkToWebsite", viewModel);
        }

        private void PopulateViewModelWithOrganisationAndReportingYear(ReportLinkToWebsiteViewModel viewModel, long organisationId, int reportingYear)
        {
            viewModel.Organisation = dataRepository.Get<Organisation>(organisationId);
            viewModel.ReportingYear = reportingYear;
        }

        private void SetValuesFromDraftReturnOrSubmittedReturn(ReportLinkToWebsiteViewModel viewModel, long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            if (draftReturn != null)
            {
                SetValuesFromDraftReturn(viewModel, draftReturn);
                return;
            }

            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            Return submittedReturn = organisation.GetReturn(reportingYear);
            if (submittedReturn != null)
            {
                SetValuesFromSubmittedReturn(viewModel, submittedReturn);
                return;
            }
        }

        private void SetValuesFromDraftReturn(ReportLinkToWebsiteViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.LinkToOrganisationWebsite = draftReturn.CompanyLinkToGPGInfo;
        }

        private void SetValuesFromSubmittedReturn(ReportLinkToWebsiteViewModel viewModel, Return submittedReturn)
        {
            viewModel.LinkToOrganisationWebsite = submittedReturn.CompanyLinkToGPGInfo;
        }


        [HttpPost("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/link-to-organisation-website")]
        [ValidateAntiForgeryToken]
        public IActionResult ReportLinkToWebsitePost(string encryptedOrganisationId, int reportingYear, ReportLinkToWebsiteViewModel viewModel)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            ValidateUserInput(viewModel);

            if (viewModel.HasAnyErrors())
            {
                PopulateViewModelWithOrganisationAndReportingYear(viewModel, organisationId, reportingYear);
                return View("ReportLinkToWebsite", viewModel);
            }

            SaveChangesToDraftReturn(viewModel, organisationId, reportingYear);

            string nextPageUrl = viewModel.Action == ReportPagesAction.Save
                ? Url.Action("ReportLinkToWebsiteGet", "ReportLinkToWebsite", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear})
                : Url.Action("ReportOverview", "ReportOverview", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear});
            StatusMessageHelper.SetStatusMessage(Response, "Saved changes to website link", nextPageUrl);
            return LocalRedirect(nextPageUrl);
        }

        private void ValidateUserInput(ReportLinkToWebsiteViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.LinkToOrganisationWebsite);

            if (!string.IsNullOrEmpty(viewModel.LinkToOrganisationWebsite))
            {
                if (!UriSanitiser.IsValidHttpOrHttpsLink(viewModel.LinkToOrganisationWebsite))
                {
                    viewModel.AddErrorFor(m => m.LinkToOrganisationWebsite, "Enter a valid URL, starting with http:// or https://");
                }
            }
        }

        private void SaveChangesToDraftReturn(ReportLinkToWebsiteViewModel viewModel, long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = draftReturnService.GetOrCreateDraftReturn(organisationId, reportingYear);

            draftReturn.CompanyLinkToGPGInfo = viewModel.LinkToOrganisationWebsite;

            draftReturnService.SaveDraftReturnOrDeleteIfNotRelevent(draftReturn);
        }

    }
}
