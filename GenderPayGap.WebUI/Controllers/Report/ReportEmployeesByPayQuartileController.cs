using GenderPayGap.Core.Helpers;
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
    public class ReportEmployeesByPayQuartileController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly DraftReturnService draftReturnService;

        public ReportEmployeesByPayQuartileController(
            IDataRepository dataRepository,
            DraftReturnService draftReturnService)
        {
            this.dataRepository = dataRepository;
            this.draftReturnService = draftReturnService;
        }


        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/employees-by-pay-quartile")]
        public IActionResult EmployeesByPayQuartileGet(string encryptedOrganisationId, int reportingYear)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            var viewModel = new ReportEmployeesByPayQuartileViewModel();
            PopulateViewModel(viewModel, organisationId, reportingYear);
            SetFigures(viewModel, organisationId, reportingYear);

            return View("ReportEmployeesByPayQuartile", viewModel);
        }

        private void PopulateViewModel(ReportEmployeesByPayQuartileViewModel viewModel, long organisationId, int reportingYear)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);

            viewModel.Organisation = organisation;
            viewModel.ReportingYear = reportingYear;

            Return submittedReturn = organisation.GetReturn(reportingYear);
            bool isEditingSubmittedReturn = submittedReturn != null;
            viewModel.IsEditingSubmittedReturn = isEditingSubmittedReturn;
        }

        private void SetFigures(ReportEmployeesByPayQuartileViewModel viewModel, long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            if (draftReturn != null)
            {
                SetFiguresFromDraftReturn(viewModel, draftReturn);
                return;
            }

            Return submittedReturn = viewModel.Organisation.GetReturn(reportingYear);
            if (submittedReturn != null)
            {
                SetFiguresFromSubmittedReturn(viewModel, submittedReturn);
                return;
            }
        }

        private void SetFiguresFromDraftReturn(ReportEmployeesByPayQuartileViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.MaleUpperPayBand = draftReturn.MaleUpperQuartilePayBand;
            viewModel.FemaleUpperPayBand = draftReturn.FemaleUpperQuartilePayBand;
            viewModel.MaleUpperMiddlePayBand = draftReturn.MaleUpperPayBand;
            viewModel.FemaleUpperMiddlePayBand = draftReturn.FemaleUpperPayBand;
            viewModel.MaleLowerMiddlePayBand = draftReturn.MaleMiddlePayBand;
            viewModel.FemaleLowerMiddlePayBand = draftReturn.FemaleMiddlePayBand;
            viewModel.MaleLowerPayBand = draftReturn.MaleLowerPayBand;
            viewModel.FemaleLowerPayBand = draftReturn.FemaleLowerPayBand;
        }

        private void SetFiguresFromSubmittedReturn(ReportEmployeesByPayQuartileViewModel viewModel, Return submittedReturn)
        {
            viewModel.MaleUpperPayBand = submittedReturn.MaleUpperQuartilePayBand;
            viewModel.FemaleUpperPayBand = submittedReturn.FemaleUpperQuartilePayBand;
            viewModel.MaleUpperMiddlePayBand = submittedReturn.MaleUpperPayBand;
            viewModel.FemaleUpperMiddlePayBand = submittedReturn.FemaleUpperPayBand;
            viewModel.MaleLowerMiddlePayBand = submittedReturn.MaleMiddlePayBand;
            viewModel.FemaleLowerMiddlePayBand = submittedReturn.FemaleMiddlePayBand;
            viewModel.MaleLowerPayBand = submittedReturn.MaleLowerPayBand;
            viewModel.FemaleLowerPayBand = submittedReturn.FemaleLowerPayBand;
        }


        [HttpPost("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/employees-by-pay-quartile")]
        [ValidateAntiForgeryToken]
        public IActionResult EmployeesByPayQuartilePost(string encryptedOrganisationId, int reportingYear, ReportEmployeesByPayQuartileViewModel viewModel)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            ValidateUserInput(viewModel);

            if (viewModel.HasAnyErrors())
            {
                PopulateViewModel(viewModel, organisationId, reportingYear);
                return View("ReportEmployeesByPayQuartile", viewModel);
            }

            SaveChangesToDraftReturn(viewModel, organisationId, reportingYear);

            string nextPageUrl = viewModel.Action == ReportPagesAction.Save
                ? Url.Action("EmployeesByPayQuartileGet", "ReportEmployeesByPayQuartile", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear})
                : Url.Action("ReportOverview", "ReportOverview", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear});
            StatusMessageHelper.SetStatusMessage(Response, "Saved changes to employees by pay quarter", nextPageUrl);
            return LocalRedirect(nextPageUrl);
        }

        private void ValidateUserInput(ReportEmployeesByPayQuartileViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.MaleUpperPayBand);
            viewModel.ParseAndValidateParameters(Request, m => m.FemaleUpperPayBand);
            viewModel.ParseAndValidateParameters(Request, m => m.MaleUpperMiddlePayBand);
            viewModel.ParseAndValidateParameters(Request, m => m.FemaleUpperMiddlePayBand);
            viewModel.ParseAndValidateParameters(Request, m => m.MaleLowerMiddlePayBand);
            viewModel.ParseAndValidateParameters(Request, m => m.FemaleLowerMiddlePayBand);
            viewModel.ParseAndValidateParameters(Request, m => m.MaleLowerPayBand);
            viewModel.ParseAndValidateParameters(Request, m => m.FemaleLowerPayBand);

            // Validate percents add up to 100
            string errorMessage = "Figures for each quarter must add up to 100%";

            if (viewModel.FemaleUpperPayBand.HasValue
                && viewModel.MaleUpperPayBand.HasValue
                && viewModel.FemaleUpperPayBand.Value + viewModel.MaleUpperPayBand.Value != 100)
            {
                viewModel.AddErrorFor(m => m.FemaleUpperPayBand, errorMessage);
                viewModel.AddErrorFor(m => m.MaleUpperPayBand, errorMessage);
            }

            if (viewModel.FemaleUpperMiddlePayBand.HasValue
                && viewModel.MaleUpperMiddlePayBand.HasValue
                && viewModel.FemaleUpperMiddlePayBand.Value + viewModel.MaleUpperMiddlePayBand.Value != 100)
            {
                viewModel.AddErrorFor(m => m.FemaleUpperMiddlePayBand, errorMessage);
                viewModel.AddErrorFor(m => m.MaleUpperMiddlePayBand, errorMessage);
            }

            if (viewModel.FemaleLowerMiddlePayBand.HasValue
                && viewModel.MaleLowerMiddlePayBand.HasValue
                && viewModel.FemaleLowerMiddlePayBand.Value + viewModel.MaleLowerMiddlePayBand.Value != 100)
            {
                viewModel.AddErrorFor(m => m.FemaleLowerMiddlePayBand, errorMessage);
                viewModel.AddErrorFor(m => m.MaleLowerMiddlePayBand, errorMessage);
            }

            if (viewModel.FemaleLowerPayBand.HasValue
                && viewModel.MaleLowerPayBand.HasValue
                && viewModel.FemaleLowerPayBand.Value + viewModel.MaleLowerPayBand.Value != 100)
            {
                viewModel.AddErrorFor(m => m.FemaleLowerPayBand, errorMessage);
                viewModel.AddErrorFor(m => m.MaleLowerPayBand, errorMessage);
            }
        }

        private void SaveChangesToDraftReturn(ReportEmployeesByPayQuartileViewModel viewModel, long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = draftReturnService.GetOrCreateDraftReturn(organisationId, reportingYear);

            draftReturn.MaleUpperQuartilePayBand = viewModel.MaleUpperPayBand;
            draftReturn.FemaleUpperQuartilePayBand = viewModel.FemaleUpperPayBand;
            draftReturn.MaleUpperPayBand = viewModel.MaleUpperMiddlePayBand;
            draftReturn.FemaleUpperPayBand = viewModel.FemaleUpperMiddlePayBand;
            draftReturn.MaleMiddlePayBand = viewModel.MaleLowerMiddlePayBand;
            draftReturn.FemaleMiddlePayBand = viewModel.FemaleLowerMiddlePayBand;
            draftReturn.MaleLowerPayBand = viewModel.MaleLowerPayBand;
            draftReturn.FemaleLowerPayBand = viewModel.FemaleLowerPayBand;

            draftReturnService.SaveDraftReturnOrDeleteIfNotRelevent(draftReturn);
        }

    }
}
