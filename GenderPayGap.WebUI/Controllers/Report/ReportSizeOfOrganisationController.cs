using GenderPayGap.Core.Helpers;
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
    [Route("account/employers")]
    public class ReportSizeOfOrganisationController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly DraftReturnService draftReturnService;

        public ReportSizeOfOrganisationController(
            IDataRepository dataRepository,
            DraftReturnService draftReturnService)
        {
            this.dataRepository = dataRepository;
            this.draftReturnService = draftReturnService;
        }
        
        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/size-of-organisation")]
        public IActionResult ReportSizeOfOrganisationGet(string encryptedOrganisationId, int reportingYear)
        {
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            var viewModel = new ReportSizeOfOrganisationViewModel();
            PopulateViewModel(viewModel, organisationId, reportingYear);
            SetValuesFromDraftReturnOrSubmittedReturn(viewModel, organisationId, reportingYear);

            return View("ReportSizeOfOrganisation", viewModel);
        }
        
        private void PopulateViewModel(ReportSizeOfOrganisationViewModel viewModel, long organisationId, int reportingYear)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);

            viewModel.Organisation = organisation;
            viewModel.ReportingYear = reportingYear;
            
            viewModel.IsEditingSubmittedReturn = ReportHelper.HasSubmittedReturn(organisation, reportingYear);
        }
        
        private void SetValuesFromDraftReturnOrSubmittedReturn(ReportSizeOfOrganisationViewModel viewModel, long organisationId, int reportingYear)
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

        private void SetValuesFromDraftReturn(ReportSizeOfOrganisationViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.SetSizeOfOrganisation(draftReturn.OrganisationSize);
        }

        private void SetValuesFromSubmittedReturn(ReportSizeOfOrganisationViewModel viewModel, Return submittedReturn)
        {
            viewModel.SetSizeOfOrganisation(submittedReturn.OrganisationSize);
        }
        
        
        [HttpPost("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/size-of-organisation")]
        [ValidateAntiForgeryToken]
        public IActionResult ReportSizeOfOrganisationPost(string encryptedOrganisationId, int reportingYear, ReportSizeOfOrganisationViewModel viewModel)
        {
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            ValidateUserInput(viewModel);

            if (viewModel.HasAnyErrors())
            {
                PopulateViewModel(viewModel, organisationId, reportingYear);
                return View("ReportSizeOfOrganisation", viewModel);
            }

            SaveChangesToDraftReturn(viewModel, organisationId, reportingYear);

            string nextPageUrl = Url.Action("ReportOverview", "ReportOverview", new { encryptedOrganisationId, reportingYear});
            StatusMessageHelper.SetStatusMessage(Response, "Saved changes to draft", nextPageUrl);
            return LocalRedirect(nextPageUrl);
        }
        
        private void ValidateUserInput(ReportSizeOfOrganisationViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.SizeOfOrganisation);
        }

        private void SaveChangesToDraftReturn(ReportSizeOfOrganisationViewModel viewModel, long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = draftReturnService.GetOrCreateDraftReturn(organisationId, reportingYear);

            draftReturn.OrganisationSize = viewModel.GetSizeOfOrganisation();

            draftReturnService.SaveDraftReturnOrDeleteIfNotRelevant(draftReturn);
        }

    }
}
