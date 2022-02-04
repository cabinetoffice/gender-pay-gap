﻿using GenderPayGap.Core;
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
    [Route("account/organisations")]
    public class NewReportResponsiblePersonController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly DraftReturnService draftReturnService;

        public NewReportResponsiblePersonController(
            IDataRepository dataRepository,
            DraftReturnService draftReturnService)
        {
            this.dataRepository = dataRepository;
            this.draftReturnService = draftReturnService;
        }
        
        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/new-responsible-person")]
        public IActionResult NewReportResponsiblePersonGet(string encryptedOrganisationId, int reportingYear)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            if (organisation.SectorType == SectorTypes.Public)
            {
                string nextPageUrl = Url.Action("NewReportOverview", "NewReportOverview", new { encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear });
                StatusMessageHelper.SetStatusMessage(Response, "Public authority employers are not required to provide a person responsible", nextPageUrl);
                return LocalRedirect(nextPageUrl);
            }

            var viewModel = new ReportResponsiblePersonViewModel();
            PopulateViewModel(viewModel, organisationId, reportingYear);
            SetValuesFromDraftReturnOrSubmittedReturn(viewModel, organisationId, reportingYear);

            return View("~/Views/ReportResponsiblePerson/NewReportResponsiblePerson.cshtml", viewModel);
        }
        
        private void PopulateViewModel(ReportResponsiblePersonViewModel viewModel, long organisationId, int reportingYear)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);

            viewModel.Organisation = organisation;
            viewModel.ReportingYear = reportingYear;

            Return submittedReturn = organisation.GetReturn(reportingYear);
            bool isEditingSubmittedReturn = submittedReturn != null;
            viewModel.IsEditingSubmittedReturn = isEditingSubmittedReturn;
        }
        
        private void SetValuesFromDraftReturnOrSubmittedReturn(ReportResponsiblePersonViewModel viewModel, long organisationId, int reportingYear)
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
        
        
        [HttpPost("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/new-responsible-person")]
        [ValidateAntiForgeryToken]
        public IActionResult NewReportResponsiblePersonPost(string encryptedOrganisationId, int reportingYear, ReportResponsiblePersonViewModel viewModel)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            if (organisation.SectorType == SectorTypes.Public)
            {
                string nextPagePublicSectorUrl = Url.Action("NewReportOverview", "NewReportOverview", new { encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear });
                StatusMessageHelper.SetStatusMessage(Response, "Public authority employers are not required to provide a person responsible", nextPagePublicSectorUrl);
                return LocalRedirect(nextPagePublicSectorUrl);
            }

            ValidateUserInput(viewModel);

            if (viewModel.HasAnyErrors())
            {
                PopulateViewModel(viewModel, organisationId, reportingYear);
                return View("~/Views/ReportResponsiblePerson/NewReportResponsiblePerson.cshtml", viewModel);
            }

            SaveChangesToDraftReturn(viewModel, organisationId, reportingYear);

            string nextPageUrl = Url.Action("NewReportOverview", "NewReportOverview", new {encryptedOrganisationId = encryptedOrganisationId, reportingYear = reportingYear});
            StatusMessageHelper.SetStatusMessage(Response, "Saved changes to draft", nextPageUrl);
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
