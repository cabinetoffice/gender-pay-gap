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
    public class ReportLinkToWebsiteController: Controller
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
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear, organisationId, dataRepository);

            var viewModel = new ReportLinkToWebsiteViewModel();
            PopulateViewModel(viewModel, organisationId, reportingYear);
            SetValuesFromDraftReturnOrSubmittedReturn(viewModel, organisationId, reportingYear);

            return View("ReportLinkToWebsite", viewModel);
        }
        
        private void PopulateViewModel(ReportLinkToWebsiteViewModel viewModel, long organisationId, int reportingYear)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);

            viewModel.Organisation = organisation;
            viewModel.ReportingYear = reportingYear;
            
            viewModel.IsEditingSubmittedReturn = organisation.HasSubmittedReturn(reportingYear);
        }
        
        private void SetValuesFromDraftReturnOrSubmittedReturn(ReportLinkToWebsiteViewModel viewModel, long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            if (draftReturn != null)
            {
                SetValuesFromDraftReturn(viewModel, draftReturn);
                return;
            }

            Return submittedReturn = viewModel.Organisation.GetReturn(reportingYear);
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
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear, organisationId, dataRepository);

            ValidateUserInput(viewModel);

            if (!ModelState.IsValid)
            {
                PopulateViewModel(viewModel, organisationId, reportingYear);
                return View("ReportLinkToWebsite", viewModel);
            }

            SaveChangesToDraftReturn(viewModel, organisationId, reportingYear);

            string nextPageUrl = Url.Action("ReportOverview", "ReportOverview", new { encryptedOrganisationId, reportingYear});
            StatusMessageHelper.SetStatusMessage(Response, "Saved changes to draft", nextPageUrl);
            return LocalRedirect(nextPageUrl);
        }

        private void ValidateUserInput(ReportLinkToWebsiteViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.LinkToOrganisationWebsite))
            {
                if (!UriSanitiser.IsValidHttpOrHttpsLink(viewModel.LinkToOrganisationWebsite))
                {
                    ModelState.AddModelError(nameof(viewModel.LinkToOrganisationWebsite), "Please enter a valid web address");
                }
            }
        }

        private void SaveChangesToDraftReturn(ReportLinkToWebsiteViewModel viewModel, long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = draftReturnService.GetOrCreateDraftReturn(organisationId, reportingYear);

            draftReturn.CompanyLinkToGPGInfo = viewModel.LinkToOrganisationWebsite;

            draftReturnService.SaveDraftReturnOrDeleteIfNotRelevant(draftReturn);
        }


    }
}
