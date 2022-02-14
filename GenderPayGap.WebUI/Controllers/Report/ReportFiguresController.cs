using GenderPayGap.Core.Classes;
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
    public class ReportFiguresController: Controller
    {
        
        private readonly IDataRepository dataRepository;
        private readonly DraftReturnService draftReturnService;

        public ReportFiguresController(
            IDataRepository dataRepository,
            DraftReturnService draftReturnService)
        {
            this.dataRepository = dataRepository;
            this.draftReturnService = draftReturnService;
        }
        
        #region GET
        
        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/figures")]
        public IActionResult ReportFiguresGet(string encryptedOrganisationId, int reportingYear)
         {
             long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
             
             ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
             ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
             ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

             var viewModel = new ReportFiguresViewModel();
             
             PopulateViewModel(viewModel, organisationId, reportingYear);
             
             DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
             Return submittedReturn = viewModel.Organisation.GetReturn(reportingYear);
             
             SetFigures(viewModel, draftReturn, submittedReturn);

             return View("~/Views/ReportFigures/ReportFigures.cshtml", viewModel);
         }

        private void PopulateViewModel(ReportFiguresViewModel viewModel, long organisationId, int reportingYear)
         {
             Organisation organisation = dataRepository.Get<Organisation>(organisationId);
             Return submittedReturn = organisation.GetReturn(reportingYear);
             bool isEditingSubmittedReturn = submittedReturn != null;
            
            
             viewModel.Organisation = organisation;
             viewModel.ReportingYear = reportingYear;
             viewModel.IsEditingSubmittedReturn = isEditingSubmittedReturn;
             viewModel.SnapshotDate = organisation.SectorType.GetAccountingStartDate(reportingYear);
         }

        private void SetFigures(ReportFiguresViewModel viewModel, DraftReturn draftReturn, Return submittedReturn)
         {
             if (draftReturn != null)
             {
                 SetFiguresFromDraftReturn(viewModel, draftReturn);
             }
             else if (submittedReturn != null)
             {
                 SetFiguresFromSubmittedReturn(viewModel, submittedReturn);
             }
         }

        private void SetFiguresFromDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
         {
             SetHourlyPayQuarterFiguresFromDraftReturn(viewModel, draftReturn);
             SetMeanAndMedianGenderPayGapUsingHourlyPayFiguresFromDraftReturn(viewModel, draftReturn);
             SetBonusPayFiguresFromDraftReturn(viewModel, draftReturn);
             SetOptedOutOfReportingPayQuarterFromDraftReturn(viewModel, draftReturn);
         }

        private void SetFiguresFromSubmittedReturn(ReportFiguresViewModel viewModel, Return submittedReturn)
         {
             SetHourlyPayQuarterFiguresFromSubmittedReturn(viewModel, submittedReturn);
             SetMeanAndMedianGenderPayGapUsingHourlyPayFiguresFromSubmittedReturn(viewModel, submittedReturn);
             SetBonusPayFiguresFromSubmittedReturn(viewModel, submittedReturn);
             SetOptedOutOfReportingPayQuarterFromSubmittedReturn(viewModel, submittedReturn);
         }
         
        private void SetHourlyPayQuarterFiguresFromDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
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

        private void SetHourlyPayQuarterFiguresFromSubmittedReturn(ReportFiguresViewModel viewModel, Return submittedReturn)
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

        private void SetMeanAndMedianGenderPayGapUsingHourlyPayFiguresFromDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.DiffMeanHourlyPayPercent = draftReturn.DiffMeanHourlyPayPercent;
            viewModel.DiffMedianHourlyPercent = draftReturn.DiffMedianHourlyPercent;
        }

        private void SetMeanAndMedianGenderPayGapUsingHourlyPayFiguresFromSubmittedReturn(ReportFiguresViewModel viewModel, Return submittedReturn)
        {
            viewModel.DiffMeanHourlyPayPercent = submittedReturn.DiffMeanHourlyPayPercent;
            viewModel.DiffMedianHourlyPercent = submittedReturn.DiffMedianHourlyPercent;
        }

        private void SetBonusPayFiguresFromDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.FemaleBonusPayPercent = draftReturn.FemaleMedianBonusPayPercent;
            viewModel.MaleBonusPayPercent = draftReturn.MaleMedianBonusPayPercent;
            viewModel.DiffMeanBonusPercent = draftReturn.DiffMeanBonusPercent;
            viewModel.DiffMedianBonusPercent = draftReturn.DiffMedianBonusPercent;
        }

        private void SetBonusPayFiguresFromSubmittedReturn(ReportFiguresViewModel viewModel, Return submittedReturn)
        {
            viewModel.FemaleBonusPayPercent = submittedReturn.FemaleMedianBonusPayPercent;
            viewModel.MaleBonusPayPercent = submittedReturn.MaleMedianBonusPayPercent;
            viewModel.DiffMeanBonusPercent = submittedReturn.DiffMeanBonusPercent;
            viewModel.DiffMedianBonusPercent = submittedReturn.DiffMedianBonusPercent;
        }
        
        private void SetOptedOutOfReportingPayQuarterFromDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.OptedOutOfReportingPayQuarters = draftReturn.OptedOutOfReportingPayQuarters;
        }
        
        private void SetOptedOutOfReportingPayQuarterFromSubmittedReturn(ReportFiguresViewModel viewModel, Return submittedReturn)
        {
            viewModel.OptedOutOfReportingPayQuarters = submittedReturn.OptedOutOfReportingPayQuarters;
        }

        #endregion

        #region POST

        [HttpPost("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/figures")]
        [ValidateAntiForgeryToken]
        public IActionResult ReportFiguresPost(string encryptedOrganisationId, int reportingYear, ReportFiguresViewModel viewModel)
        {
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            ValidateUserInput(viewModel);

            if (viewModel.HasAnyErrors())
            {
                PopulateViewModel(viewModel, organisationId, reportingYear);
                return View("~/Views/ReportFigures/ReportFigures.cshtml", viewModel);
            }

            SaveChangesToDraftReturn(viewModel, organisationId, reportingYear);

            string nextPageUrl = Url.Action("ReportOverview", "ReportOverview", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear});
            StatusMessageHelper.SetStatusMessage(Response, "Saved changes to draft", nextPageUrl);
            return LocalRedirect(nextPageUrl);
        }

        private void ValidateUserInput(ReportFiguresViewModel viewModel)
        {
            ValidateBonusPayFigures(viewModel);
            ValidateHourlyPayFigures(viewModel);

            if (!viewModel.OptedOutOfReportingPayQuarters)
            {
                ValidatePayQuartileFigures(viewModel);
            }
        }

        private void SaveChangesToDraftReturn(ReportFiguresViewModel viewModel, long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = draftReturnService.GetOrCreateDraftReturn(organisationId, reportingYear);
            
            SaveBonusPayFiguresToDraftReturn(viewModel, draftReturn);
            SaveHourlyPayFiguresToDraftReturn(viewModel, draftReturn);
            SaveOptedOutOdReportingPayQuartersToDraftReturn(viewModel, draftReturn);
            SavePayQuartileFiguresToDraftReturn(viewModel, draftReturn);
            
            draftReturnService.SaveDraftReturnOrDeleteIfNotRelevent(draftReturn);
        }

        private void ValidateBonusPayFigures(ReportFiguresViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.FemaleBonusPayPercent);
            viewModel.ParseAndValidateParameters(Request, m => m.MaleBonusPayPercent);
            viewModel.ParseAndValidateParameters(Request, m => m.DiffMeanBonusPercent);
            viewModel.ParseAndValidateParameters(Request, m => m.DiffMedianBonusPercent);
        }

        private void ValidatePayQuartileFigures(ReportFiguresViewModel viewModel)
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

        private void ValidateHourlyPayFigures(ReportFiguresViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.DiffMeanHourlyPayPercent);
            viewModel.ParseAndValidateParameters(Request, m => m.DiffMedianHourlyPercent);
        }
        
        private void SavePayQuartileFiguresToDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
        {
            draftReturn.MaleUpperQuartilePayBand = viewModel.MaleUpperPayBand;
            draftReturn.FemaleUpperQuartilePayBand = viewModel.FemaleUpperPayBand;
            draftReturn.MaleUpperPayBand = viewModel.MaleUpperMiddlePayBand;
            draftReturn.FemaleUpperPayBand = viewModel.FemaleUpperMiddlePayBand;
            draftReturn.MaleMiddlePayBand = viewModel.MaleLowerMiddlePayBand;
            draftReturn.FemaleMiddlePayBand = viewModel.FemaleLowerMiddlePayBand;
            draftReturn.MaleLowerPayBand = viewModel.MaleLowerPayBand;
            draftReturn.FemaleLowerPayBand = viewModel.FemaleLowerPayBand;
        }
        
        private void SaveBonusPayFiguresToDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
        {
            draftReturn.FemaleMedianBonusPayPercent = viewModel.FemaleBonusPayPercent;
            draftReturn.MaleMedianBonusPayPercent = viewModel.MaleBonusPayPercent;
            draftReturn.DiffMeanBonusPercent = viewModel.DiffMeanBonusPercent;
            draftReturn.DiffMedianBonusPercent = viewModel.DiffMedianBonusPercent;
        }

        private void SaveHourlyPayFiguresToDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
        {
            draftReturn.DiffMeanHourlyPayPercent = viewModel.DiffMeanHourlyPayPercent;
            draftReturn.DiffMedianHourlyPercent = viewModel.DiffMedianHourlyPercent;
        }

        private void SaveOptedOutOdReportingPayQuartersToDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
        {
            draftReturn.OptedOutOfReportingPayQuarters = viewModel.OptedOutOfReportingPayQuarters;
        }
        
        #endregion

    }
}
