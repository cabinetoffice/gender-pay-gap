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
    public class ReportResponsiblePersonController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly DraftReturnService draftReturnService;

        public ReportResponsiblePersonController(
            IDataRepository dataRepository,
            DraftReturnService draftReturnService)
        {
            this.dataRepository = dataRepository;
            this.draftReturnService = draftReturnService;
        }


        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/responsible-person")]
        public IActionResult ReportResponsiblePersonGet(string encryptedOrganisationId, int reportingYear)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            var viewModel = new ReportResponsiblePersonViewModel();
            PopulateViewModelWithOrganisationAndReportingYear(viewModel, organisationId, reportingYear);
            SetValuesFromDraftReturnOrSubmittedReturn(viewModel, organisationId, reportingYear);

            return View("ReportResponsiblePerson", viewModel);
        }

        private void PopulateViewModelWithOrganisationAndReportingYear(ReportResponsiblePersonViewModel viewModel, long organisationId, int reportingYear)
        {
            viewModel.Organisation = dataRepository.Get<Organisation>(organisationId);
            viewModel.ReportingYear = reportingYear;
        }

        private void SetValuesFromDraftReturnOrSubmittedReturn(ReportResponsiblePersonViewModel viewModel, long organisationId, int reportingYear)
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

        private void SetValuesFromDraftReturn(ReportResponsiblePersonViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.ResponsiblePersonFirstName = draftReturn.FirstName;
            viewModel.ResponsiblePersonLastName = draftReturn.LastName;
            viewModel.ResponsiblePersonJobTitle = draftReturn.JobTitle;
        }

        private void SetValuesFromSubmittedReturn(ReportResponsiblePersonViewModel viewModel, Return submittedReturn)
        {
            viewModel.ResponsiblePersonFirstName = submittedReturn.FirstName;
            viewModel.ResponsiblePersonLastName = submittedReturn.LastName;
            viewModel.ResponsiblePersonJobTitle = submittedReturn.JobTitle;
        }


        [HttpPost("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/responsible-person")]
        [ValidateAntiForgeryToken]
        public IActionResult ReportResponsiblePersonPost(string encryptedOrganisationId, int reportingYear, ReportResponsiblePersonViewModel viewModel)
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
                return View("ReportResponsiblePerson", viewModel);
            }

            SaveChangesToDraftReturn(viewModel, organisationId, reportingYear);

            string nextPageUrl = viewModel.Action == ReportPagesAction.Save
                ? Url.Action("ReportResponsiblePersonGet", "ReportResponsiblePerson", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear})
                : Url.Action("ReportOverview", "ReportOverview", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear});
            StatusMessageHelper.SetStatusMessage(Response, "Saved changes to person responsible in your organisation", nextPageUrl);
            return LocalRedirect(nextPageUrl);
        }

        private void ValidateUserInput(ReportResponsiblePersonViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.ResponsiblePersonFirstName);
            viewModel.ParseAndValidateParameters(Request, m => m.ResponsiblePersonLastName);
            viewModel.ParseAndValidateParameters(Request, m => m.ResponsiblePersonJobTitle);
        }

        private void SaveChangesToDraftReturn(ReportResponsiblePersonViewModel viewModel, long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = draftReturnService.GetOrCreateDraftReturn(organisationId, reportingYear);

            draftReturn.FirstName = viewModel.ResponsiblePersonFirstName;
            draftReturn.LastName = viewModel.ResponsiblePersonLastName;
            draftReturn.JobTitle = viewModel.ResponsiblePersonJobTitle;

            draftReturnService.SaveDraftReturnOrDeleteIfNotRelevent(draftReturn);
        }

    }
}
