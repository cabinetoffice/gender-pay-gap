using GenderPayGap.Core;
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
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            var viewModel = new ReportSizeOfOrganisationViewModel();
            PopulateViewModelWithOrganisationAndReportingYear(viewModel, organisationId, reportingYear);
            SetValuesFromDraftReturnOrSubmittedReturn(viewModel, organisationId, reportingYear);

            return View("ReportSizeOfOrganisation", viewModel);
        }

        private void PopulateViewModelWithOrganisationAndReportingYear(ReportSizeOfOrganisationViewModel viewModel, long organisationId, int reportingYear)
        {
            viewModel.Organisation = dataRepository.Get<Organisation>(organisationId);
            viewModel.ReportingYear = reportingYear;
        }

        private void SetValuesFromDraftReturnOrSubmittedReturn(ReportSizeOfOrganisationViewModel viewModel, long organisationId, int reportingYear)
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
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            ValidateUserInput(viewModel);

            if (viewModel.HasAnyErrors())
            {
                PopulateViewModelWithOrganisationAndReportingYear(viewModel, organisationId, reportingYear);
                return View("ReportSizeOfOrganisation", viewModel);
            }

            SaveChangesToDraftReturn(viewModel, organisationId, reportingYear);

            string nextPageUrl = viewModel.Action == ReportPagesAction.Save
                ? Url.Action("ReportSizeOfOrganisationGet", "ReportSizeOfOrganisation", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear})
                : Url.Action("ReportOverview", "ReportOverview", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear});
            StatusMessageHelper.SetStatusMessage(Response, "Saved changes to size of organisation", nextPageUrl);
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

            draftReturnService.SaveDraftReturnOrDeleteIfNotRelevent(draftReturn);
        }

    }
}
