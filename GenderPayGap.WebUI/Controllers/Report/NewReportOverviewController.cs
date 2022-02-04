using System;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Report;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Report
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("account/organisations")]
    public class NewReportOverviewController: Controller
    {
        
        private readonly IDataRepository dataRepository;
        private readonly DraftReturnService draftReturnService;
        private readonly ReturnService returnService;

        public NewReportOverviewController(
            IDataRepository dataRepository,
            DraftReturnService draftReturnService,
            ReturnService returnService)
        {
            this.dataRepository = dataRepository;
            this.draftReturnService = draftReturnService;
            this.returnService = returnService;
        }
        
        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/submit")]
        public IActionResult SubmitReturnGet(string encryptedOrganisationId, int reportingYear)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);
            
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);
            
            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            
            if (!draftReturnService.DraftReturnExistsAndRequiredFieldsAreComplete(organisationId, reportingYear))
            {
                return RedirectToReportOverviewPage(encryptedOrganisationId, reportingYear, "This report is not ready to submit. Complete the remaining sections");
            }
            
            
            if (WillBeLateSubmission(draftReturn))
            {
                // Late submission Reason
                return RedirectToAction("NewLateSubmissionReasonGet", "NewLateSubmission", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear});
            }
            else
            {
                User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
                Return newReturn = returnService.CreateAndSaveReturnFromDraftReturn(draftReturn, user, Url);

                // Confirmation
                return RedirectToAction("NewReportConfirmation", "NewReportConfirmation",
                    new
                    {
                        encryptedOrganisationId = encryptedOrganisationId,
                        reportingYear = reportingYear,
                        confirmationId = Encryption.EncryptQuerystring(newReturn.ReturnId.ToString())
                    });
            }
        }

        private IActionResult RedirectToReportOverviewPage(string encryptedOrganisationId, int reportingYear, string message)
        {
            string nextPageUrl = Url.Action("NewReportOverview", "NewReportOverview", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear});
            StatusMessageHelper.SetStatusMessage(Response, message, nextPageUrl);
            return LocalRedirect(nextPageUrl);
        }
        
        private bool WillBeLateSubmission(DraftReturn draftReturn)
        {
            return draftReturnService.DraftReturnWouldBeNewlyLateIfSubmittedNow(draftReturn);
        }
        
        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report-new")]
        public IActionResult NewReportOverview(string encryptedOrganisationId, int reportingYear, bool shouldShowLateSubmissionWarning = false)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            if (ReportIsLate(organisationId, reportingYear) && shouldShowLateSubmissionWarning)
            {
                return RedirectToAction("NewLateSubmissionWarningGet", "NewLateSubmission", new {encryptedOrganisationId, reportingYear});
            }
            
            var viewModel = new NewReportOverviewViewModel();
            PopulateViewModel(viewModel, organisationId, reportingYear);
            
            return View("~/Views/ReportOverview/NewReportOverview.cshtml", viewModel);
            
        }

        private bool ReportIsLate(long organisationId, int reportingYear)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            DateTime snapshotDate = organisation.SectorType.GetAccountingStartDate(reportingYear);

            // The deadline date is the final day that a return can be submitted without being considered late
            // The due date is a day later, the point at which a return is considered late
            // i.e. if the deadline date is 2021/04/01, submissions on that day are not late, any after 2021/04/02 00:00:00 are
            DateTime dueDate = ReportingYearsHelper.GetDeadlineForAccountingDate(snapshotDate).AddDays(1);
            bool isLate = VirtualDateTime.Now > dueDate;
            bool isInScope = organisation.GetScopeForYear(reportingYear).IsInScopeVariant();
            bool yearIsNotExcluded = !Global.ReportingStartYearsToExcludeFromLateFlagEnforcement.Contains(reportingYear);

            return isLate && isInScope && yearIsNotExcluded;
        }
        
        private void PopulateViewModel(NewReportOverviewViewModel viewModel, long organisationId, int reportingYear)
        {
            SetOrganisationInformation(viewModel, organisationId, reportingYear);
            
            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            Return submittedReturn = viewModel.Organisation.GetReturn(reportingYear);
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
        
        private void SetOrganisationInformation(NewReportOverviewViewModel viewModel, long organisationId, int reportingYear)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            Return submittedReturn = organisation.GetReturn(reportingYear);
            bool isEditingSubmittedReturn = submittedReturn != null;
            
            
            viewModel.Organisation = organisation;
            viewModel.ReportingYear = reportingYear;
            viewModel.IsEditingSubmittedReturn = isEditingSubmittedReturn;
            viewModel.SnapshotDate = organisation.SectorType.GetAccountingStartDate(reportingYear);
        }

        private void SetValuesFromDraftReturn(NewReportOverviewViewModel viewModel, DraftReturn draftReturn)
        {
            SetHourlyPayQuarterFiguresFromDraftReturn(viewModel, draftReturn);
            SetMeanAndMedianGenderPayGapUsingHourlyPayFiguresFromDraftReturn(viewModel, draftReturn);
            SetBonusPayFiguresFromDraftReturn(viewModel, draftReturn);
            SetPersonResponsibleFromDraftReturn(viewModel, draftReturn);
            
            SetOrganisationSizeFromDraftReturn(viewModel, draftReturn);
            SetLinkToGenderPayGapInformationFromDraftReturn(viewModel, draftReturn);

            SetOptedOutOfReportingPayQuarterFromDraftReturn(viewModel, draftReturn);

        }

        private void SetValuesFromSubmittedReturn(NewReportOverviewViewModel viewModel, Return submittedReturn)
        {
            SetHourlyPayQuarterFiguresFromSubmittedReturn(viewModel, submittedReturn);
            SetMeanAndMedianGenderPayGapUsingHourlyPayFiguresFromSubmittedReturn(viewModel, submittedReturn);
            SetBonusPayFiguresFromSubmittedReturn(viewModel, submittedReturn);
            SetPersonResponsibleFromSubmittedReturn(viewModel, submittedReturn);

            
            SetOrganisationSizeFromSubmittedReturn(viewModel, submittedReturn);
            SetLinkToGenderPayGapInformationFromSubmittedReturn(viewModel, submittedReturn);

            SetOptedOutOfReportingPayQuarterFromSubmittedReturn(viewModel, submittedReturn);
        }

        private void SetHourlyPayQuarterFiguresFromDraftReturn(NewReportOverviewViewModel viewModel, DraftReturn draftReturn)
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

        private void SetHourlyPayQuarterFiguresFromSubmittedReturn(NewReportOverviewViewModel viewModel, Return submittedReturn)
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

        private void SetMeanAndMedianGenderPayGapUsingHourlyPayFiguresFromDraftReturn(NewReportOverviewViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.DiffMeanHourlyPayPercent = draftReturn.DiffMeanHourlyPayPercent;
            viewModel.DiffMedianHourlyPercent = draftReturn.DiffMedianHourlyPercent;
        }

        private void SetMeanAndMedianGenderPayGapUsingHourlyPayFiguresFromSubmittedReturn(NewReportOverviewViewModel viewModel, Return submittedReturn)
        {
            viewModel.DiffMeanHourlyPayPercent = submittedReturn.DiffMeanHourlyPayPercent;
            viewModel.DiffMedianHourlyPercent = submittedReturn.DiffMedianHourlyPercent;
        }

        private void SetBonusPayFiguresFromDraftReturn(NewReportOverviewViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.FemaleBonusPayPercent = draftReturn.FemaleMedianBonusPayPercent;
            viewModel.MaleBonusPayPercent = draftReturn.MaleMedianBonusPayPercent;
            viewModel.DiffMeanBonusPercent = draftReturn.DiffMeanBonusPercent;
            viewModel.DiffMedianBonusPercent = draftReturn.DiffMedianBonusPercent;
        }

        private void SetBonusPayFiguresFromSubmittedReturn(NewReportOverviewViewModel viewModel, Return submittedReturn)
        {
            viewModel.FemaleBonusPayPercent = submittedReturn.FemaleMedianBonusPayPercent;
            viewModel.MaleBonusPayPercent = submittedReturn.MaleMedianBonusPayPercent;
            viewModel.DiffMeanBonusPercent = submittedReturn.DiffMeanBonusPercent;
            viewModel.DiffMedianBonusPercent = submittedReturn.DiffMedianBonusPercent;
        }

        private void SetPersonResponsibleFromDraftReturn(NewReportOverviewViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.ResponsiblePersonFirstName = draftReturn.FirstName;
            viewModel.ResponsiblePersonLastName = draftReturn.LastName;
            viewModel.ResponsiblePersonJobTitle = draftReturn.JobTitle;
        }

        private void SetPersonResponsibleFromSubmittedReturn(NewReportOverviewViewModel viewModel, Return submittedReturn)
        {
            viewModel.ResponsiblePersonFirstName = submittedReturn.FirstName;
            viewModel.ResponsiblePersonLastName = submittedReturn.LastName;
            viewModel.ResponsiblePersonJobTitle = submittedReturn.JobTitle;
        }

        private void SetOrganisationSizeFromDraftReturn(NewReportOverviewViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.SizeOfOrganisation = draftReturn.OrganisationSize;
        }

        private void SetOrganisationSizeFromSubmittedReturn(NewReportOverviewViewModel viewModel, Return submittedReturn)
        {
            viewModel.SizeOfOrganisation = submittedReturn.OrganisationSize;
        }

        private void SetLinkToGenderPayGapInformationFromDraftReturn(NewReportOverviewViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.LinkToOrganisationWebsite = draftReturn.CompanyLinkToGPGInfo;
        }

        private void SetLinkToGenderPayGapInformationFromSubmittedReturn(NewReportOverviewViewModel viewModel, Return submittedReturn)
        {
            viewModel.LinkToOrganisationWebsite = submittedReturn.CompanyLinkToGPGInfo;
        }

        private void SetOptedOutOfReportingPayQuarterFromDraftReturn(NewReportOverviewViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.OptedOutOfReportingPayQuarters = draftReturn.OptedOutOfReportingPayQuarters;
        }
        
        private void SetOptedOutOfReportingPayQuarterFromSubmittedReturn(NewReportOverviewViewModel viewModel, Return submittedReturn)
        {
            viewModel.OptedOutOfReportingPayQuarters = submittedReturn.OptedOutOfReportingPayQuarters;
        }
    }
}
