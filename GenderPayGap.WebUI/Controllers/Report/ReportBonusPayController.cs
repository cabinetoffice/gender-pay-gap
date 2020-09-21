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
    public class ReportBonusPayController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly DraftReturnService draftReturnService;

        public ReportBonusPayController(
            IDataRepository dataRepository,
            DraftReturnService draftReturnService)
        {
            this.dataRepository = dataRepository;
            this.draftReturnService = draftReturnService;
        }


        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/bonus-pay")]
        public IActionResult ReportBonusPayGet(string encryptedOrganisationId, int reportingYear)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            var viewModel = new ReportBonusPayViewModel();
            PopulateViewModelWithOrganisationAndReportingYear(viewModel, organisationId, reportingYear);
            SetFigures(viewModel, organisationId, reportingYear);

            return View("ReportBonusPay", viewModel);
        }

        private void PopulateViewModelWithOrganisationAndReportingYear(ReportBonusPayViewModel viewModel, long organisationId, int reportingYear)
        {
            viewModel.Organisation = dataRepository.Get<Organisation>(organisationId);
            viewModel.ReportingYear = reportingYear;
        }

        private void SetFigures(ReportBonusPayViewModel viewModel, long organisationId, int reportingYear)
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

        private void SetFiguresFromDraftReturn(ReportBonusPayViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.FemaleBonusPayPercent = draftReturn.FemaleMedianBonusPayPercent;
            viewModel.MaleBonusPayPercent = draftReturn.MaleMedianBonusPayPercent;
            viewModel.DiffMeanBonusPercent = draftReturn.DiffMeanBonusPercent;
            viewModel.DiffMedianBonusPercent = draftReturn.DiffMedianBonusPercent;
        }

        private void SetFiguresFromSubmittedReturn(ReportBonusPayViewModel viewModel, Return submittedReturn)
        {
            viewModel.FemaleBonusPayPercent = submittedReturn.FemaleMedianBonusPayPercent;
            viewModel.MaleBonusPayPercent = submittedReturn.MaleMedianBonusPayPercent;
            viewModel.DiffMeanBonusPercent = submittedReturn.DiffMeanBonusPercent;
            viewModel.DiffMedianBonusPercent = submittedReturn.DiffMedianBonusPercent;
        }


        [HttpPost("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/bonus-pay")]
        [ValidateAntiForgeryToken]
        public IActionResult ReportBonusPayPost(string encryptedOrganisationId, int reportingYear, ReportBonusPayViewModel viewModel)
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
                return View("ReportBonusPay", viewModel);
            }

            SaveChangesToDraftReturn(viewModel, organisationId, reportingYear);

            string nextPageUrl = viewModel.Action == ReportPagesAction.Save
                ? Url.Action("ReportBonusPayGet", "ReportBonusPay", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear})
                : Url.Action("ReportOverview", "ReportOverview", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear});
            StatusMessageHelper.SetStatusMessage(Response, "Saved changes to bonus pay", nextPageUrl);
            return LocalRedirect(nextPageUrl);
        }

        private void ValidateUserInput(ReportBonusPayViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.FemaleBonusPayPercent);
            viewModel.ParseAndValidateParameters(Request, m => m.MaleBonusPayPercent);
            viewModel.ParseAndValidateParameters(Request, m => m.DiffMeanBonusPercent);
            viewModel.ParseAndValidateParameters(Request, m => m.DiffMedianBonusPercent);
        }

        private void SaveChangesToDraftReturn(ReportBonusPayViewModel viewModel, long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = draftReturnService.GetOrCreateDraftReturn(organisationId, reportingYear);

            draftReturn.FemaleMedianBonusPayPercent = viewModel.FemaleBonusPayPercent;
            draftReturn.MaleMedianBonusPayPercent = viewModel.MaleBonusPayPercent;
            draftReturn.DiffMeanBonusPercent = viewModel.DiffMeanBonusPercent;
            draftReturn.DiffMedianBonusPercent = viewModel.DiffMedianBonusPercent;

            draftReturnService.SaveDraftReturnOrDeleteIfNotRelevent(draftReturn);
        }

    }
}
