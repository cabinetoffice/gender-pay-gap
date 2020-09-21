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
    public class ReportHourlyPayController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly DraftReturnService draftReturnService;

        public ReportHourlyPayController(
            IDataRepository dataRepository,
            DraftReturnService draftReturnService)
        {
            this.dataRepository = dataRepository;
            this.draftReturnService = draftReturnService;
        }


        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/hourly-pay")]
        public IActionResult ReportHourlyPayGet(string encryptedOrganisationId, int reportingYear)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            var viewModel = new ReportHourlyPayViewModel();
            PopulateViewModelWithOrganisationAndReportingYear(viewModel, organisationId, reportingYear);
            SetFigures(viewModel, organisationId, reportingYear);

            return View("ReportHourlyPay", viewModel);
        }

        private void PopulateViewModelWithOrganisationAndReportingYear(ReportHourlyPayViewModel viewModel, long organisationId, int reportingYear)
        {
            viewModel.Organisation = dataRepository.Get<Organisation>(organisationId);
            viewModel.ReportingYear = reportingYear;
        }

        private void SetFigures(ReportHourlyPayViewModel viewModel, long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            if (draftReturn != null)
            {
                SetFiguresFromDraftReturn(viewModel, draftReturn);
                return;
            }

            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            Return submittedReturn = organisation.GetReturn(reportingYear);
            if (submittedReturn != null)
            {
                SetFiguresFromSubmittedReturn(viewModel, submittedReturn);
                return;
            }
        }

        private void SetFiguresFromDraftReturn(ReportHourlyPayViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.DiffMeanHourlyPayPercent = draftReturn.DiffMeanHourlyPayPercent;
            viewModel.DiffMedianHourlyPercent = draftReturn.DiffMedianHourlyPercent;
        }

        private void SetFiguresFromSubmittedReturn(ReportHourlyPayViewModel viewModel, Return submittedReturn)
        {
            viewModel.DiffMeanHourlyPayPercent = submittedReturn.DiffMeanHourlyPayPercent;
            viewModel.DiffMedianHourlyPercent = submittedReturn.DiffMedianHourlyPercent;
        }


        [HttpPost("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/hourly-pay")]
        [ValidateAntiForgeryToken]
        public IActionResult ReportHourlyPayPost(string encryptedOrganisationId, int reportingYear, ReportHourlyPayViewModel viewModel)
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
                return View("ReportHourlyPay", viewModel);
            }

            SaveChangesToDraftReturn(viewModel, organisationId, reportingYear);

            string nextPageUrl = viewModel.Action == ReportPagesAction.Save
                ? Url.Action("ReportHourlyPayGet", "ReportHourlyPay", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear})
                : Url.Action("ReportOverview", "ReportOverview", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear});
            StatusMessageHelper.SetStatusMessage(Response, "Saved changes to hourly pay", nextPageUrl);
            return LocalRedirect(nextPageUrl);
        }

        private void ValidateUserInput(ReportHourlyPayViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.DiffMeanHourlyPayPercent);
            viewModel.ParseAndValidateParameters(Request, m => m.DiffMedianHourlyPercent);
        }

        private void SaveChangesToDraftReturn(ReportHourlyPayViewModel viewModel, long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = draftReturnService.GetOrCreateDraftReturn(organisationId, reportingYear);

            draftReturn.DiffMeanHourlyPayPercent = viewModel.DiffMeanHourlyPayPercent;
            draftReturn.DiffMedianHourlyPercent = viewModel.DiffMedianHourlyPercent;

            draftReturnService.SaveDraftReturnOrDeleteIfNotRelevent(draftReturn);
        }

    }
}
