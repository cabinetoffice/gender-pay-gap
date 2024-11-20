using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Report;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Report
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("account/organisations")]
    public class ReportOverviewController: Controller
    {
        
        private readonly IDataRepository dataRepository;
        private readonly DraftReturnService draftReturnService;
        private readonly ReturnService returnService;

        public ReportOverviewController(
            IDataRepository dataRepository,
            DraftReturnService draftReturnService,
            ReturnService returnService)
        {
            this.dataRepository = dataRepository;
            this.draftReturnService = draftReturnService;
            this.returnService = returnService;
        }
        
        [HttpPost("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/submit")]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitReturnPost(string encryptedOrganisationId, int reportingYear)
        {
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear, organisationId, dataRepository);
            
            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            
            if (!draftReturnService.DraftReturnExistsAndRequiredFieldsAreComplete(organisationId, reportingYear))
            {
                return RedirectToReportOverviewPage(encryptedOrganisationId, reportingYear, "This report is not ready to submit. Complete the remaining sections");
            }

            if (draftReturnService.DraftReturnWouldBeNewlyLateIfSubmittedNow(draftReturn))
            {
                // Late submission Reason
                return RedirectToAction("LateSubmissionReasonGet", "LateSubmission", new { encryptedOrganisationId, reportingYear});
            }
            
            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            Return newReturn = returnService.CreateAndSaveReturnFromDraftReturn(draftReturn, user, Url);

            // Confirmation
            return RedirectToAction("ReportConfirmation", "ReportConfirmation",
                new
                {
                    encryptedOrganisationId,
                    reportingYear,
                    confirmationId = Encryption.EncryptId(newReturn.ReturnId)
                });
        }

        private IActionResult RedirectToReportOverviewPage(string encryptedOrganisationId, int reportingYear, string message)
        {
            string nextPageUrl = Url.Action("ReportOverview", "ReportOverview", new { encryptedOrganisationId, reportingYear});
            StatusMessageHelper.SetStatusMessage(Response, message, nextPageUrl);
            return LocalRedirect(nextPageUrl);
        }

        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report")]
        public IActionResult ReportOverview(string encryptedOrganisationId, int reportingYear, bool canTriggerLateSubmissionWarning = false)
        {
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear, organisationId, dataRepository);

            if (draftReturnService.ShouldShowLateWarning(organisationId, reportingYear) && canTriggerLateSubmissionWarning)
            {
                return RedirectToAction("LateSubmissionWarningGet", "LateSubmission", new {encryptedOrganisationId, reportingYear});
            }

            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            Return submittedReturn = organisation.GetReturn(reportingYear);

            if (draftReturn == null && submittedReturn == null)
            {
                return RedirectToAction("ReportFiguresGet", "ReportFigures", new {encryptedOrganisationId, reportingYear});
            }
            
            
            var viewModel = new ReportOverviewViewModel();
            PopulateViewModel(viewModel, organisation, draftReturn, submittedReturn, reportingYear);
            
            return View("ReportOverview", viewModel);
        }

        private void PopulateViewModel(ReportOverviewViewModel viewModel, Organisation organisation, DraftReturn draftReturn, Return submittedReturn, int reportingYear)
        {
            SetOrganisationInformation(viewModel, organisation, reportingYear);
            viewModel.DraftReturnExists = draftReturn != null;

            if (submittedReturn != null)
            {
                SetValuesFromSubmittedReturn(viewModel, submittedReturn);
            }
            if (draftReturn != null)
            {
                SetValuesFromDraftReturn(viewModel, draftReturn);
            }
        }
        
        private void SetOrganisationInformation(ReportOverviewViewModel viewModel, Organisation organisation, int reportingYear)
        {
            viewModel.Organisation = organisation;
            viewModel.ReportingYear = reportingYear;
            viewModel.IsEditingSubmittedReturn = organisation.HasSubmittedReturn(reportingYear);
            viewModel.SnapshotDate = organisation.SectorType.GetAccountingStartDate(reportingYear);
            viewModel.SectorType = organisation.SectorType;
        }

        private void SetValuesFromDraftReturn(ReportOverviewViewModel viewModel, DraftReturn draftReturn)
        {
            SetHourlyPayQuarterFiguresFromDraftReturn(viewModel, draftReturn);
            SetMeanAndMedianGenderPayGapUsingHourlyPayFiguresFromDraftReturn(viewModel, draftReturn);
            SetBonusPayFiguresFromDraftReturn(viewModel, draftReturn);
            SetPersonResponsibleFromDraftReturn(viewModel, draftReturn);
            
            SetOrganisationSizeFromDraftReturn(viewModel, draftReturn);
            SetLinkToGenderPayGapInformationFromDraftReturn(viewModel, draftReturn);

            SetOptedOutOfReportingPayQuarterFromDraftReturn(viewModel, draftReturn);

        }

        private void SetValuesFromSubmittedReturn(ReportOverviewViewModel viewModel, Return submittedReturn)
        {
            SetHourlyPayQuarterFiguresFromSubmittedReturn(viewModel, submittedReturn);
            SetMeanAndMedianGenderPayGapUsingHourlyPayFiguresFromSubmittedReturn(viewModel, submittedReturn);
            SetBonusPayFiguresFromSubmittedReturn(viewModel, submittedReturn);
            SetPersonResponsibleFromSubmittedReturn(viewModel, submittedReturn);

            
            SetOrganisationSizeFromSubmittedReturn(viewModel, submittedReturn);
            SetLinkToGenderPayGapInformationFromSubmittedReturn(viewModel, submittedReturn);

            SetOptedOutOfReportingPayQuarterFromSubmittedReturn(viewModel, submittedReturn);
        }

        private void SetHourlyPayQuarterFiguresFromDraftReturn(ReportOverviewViewModel viewModel, DraftReturn draftReturn)
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

        private void SetHourlyPayQuarterFiguresFromSubmittedReturn(ReportOverviewViewModel viewModel, Return submittedReturn)
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

        private void SetMeanAndMedianGenderPayGapUsingHourlyPayFiguresFromDraftReturn(ReportOverviewViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.DiffMeanHourlyPayPercent = draftReturn.DiffMeanHourlyPayPercent;
            viewModel.DiffMedianHourlyPercent = draftReturn.DiffMedianHourlyPercent;
        }

        private void SetMeanAndMedianGenderPayGapUsingHourlyPayFiguresFromSubmittedReturn(ReportOverviewViewModel viewModel, Return submittedReturn)
        {
            viewModel.DiffMeanHourlyPayPercent = submittedReturn.DiffMeanHourlyPayPercent;
            viewModel.DiffMedianHourlyPercent = submittedReturn.DiffMedianHourlyPercent;
        }

        private void SetBonusPayFiguresFromDraftReturn(ReportOverviewViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.FemaleBonusPayPercent = draftReturn.FemaleMedianBonusPayPercent;
            viewModel.MaleBonusPayPercent = draftReturn.MaleMedianBonusPayPercent;
            viewModel.DiffMeanBonusPercent = draftReturn.DiffMeanBonusPercent;
            viewModel.DiffMedianBonusPercent = draftReturn.DiffMedianBonusPercent;
        }

        private void SetBonusPayFiguresFromSubmittedReturn(ReportOverviewViewModel viewModel, Return submittedReturn)
        {
            viewModel.FemaleBonusPayPercent = submittedReturn.FemaleMedianBonusPayPercent;
            viewModel.MaleBonusPayPercent = submittedReturn.MaleMedianBonusPayPercent;
            viewModel.DiffMeanBonusPercent = submittedReturn.DiffMeanBonusPercent;
            viewModel.DiffMedianBonusPercent = submittedReturn.DiffMedianBonusPercent;
        }

        private void SetPersonResponsibleFromDraftReturn(ReportOverviewViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.ResponsiblePersonFirstName = draftReturn.FirstName;
            viewModel.ResponsiblePersonLastName = draftReturn.LastName;
            viewModel.ResponsiblePersonJobTitle = draftReturn.JobTitle;
        }

        private void SetPersonResponsibleFromSubmittedReturn(ReportOverviewViewModel viewModel, Return submittedReturn)
        {
            viewModel.ResponsiblePersonFirstName = submittedReturn.FirstName;
            viewModel.ResponsiblePersonLastName = submittedReturn.LastName;
            viewModel.ResponsiblePersonJobTitle = submittedReturn.JobTitle;
        }

        private void SetOrganisationSizeFromDraftReturn(ReportOverviewViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.SizeOfOrganisation = draftReturn.OrganisationSize;
        }

        private void SetOrganisationSizeFromSubmittedReturn(ReportOverviewViewModel viewModel, Return submittedReturn)
        {
            viewModel.SizeOfOrganisation = submittedReturn.OrganisationSize;
        }

        private void SetLinkToGenderPayGapInformationFromDraftReturn(ReportOverviewViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.LinkToOrganisationWebsite = draftReturn.CompanyLinkToGPGInfo;
        }

        private void SetLinkToGenderPayGapInformationFromSubmittedReturn(ReportOverviewViewModel viewModel, Return submittedReturn)
        {
            viewModel.LinkToOrganisationWebsite = submittedReturn.CompanyLinkToGPGInfo;
        }

        private void SetOptedOutOfReportingPayQuarterFromDraftReturn(ReportOverviewViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.OptedOutOfReportingPayQuarters = draftReturn.OptedOutOfReportingPayQuarters;
        }
        
        private void SetOptedOutOfReportingPayQuarterFromSubmittedReturn(ReportOverviewViewModel viewModel, Return submittedReturn)
        {
            viewModel.OptedOutOfReportingPayQuarters = submittedReturn.OptedOutOfReportingPayQuarters;
        }
    }
}
