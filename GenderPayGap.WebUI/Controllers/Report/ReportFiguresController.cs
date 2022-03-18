using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Report;
using GenderPayGap.WebUI.Services;
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

             DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
             PopulateViewModel(viewModel, organisationId, reportingYear, draftReturn != null);

             Return submittedReturn = viewModel.Organisation.GetReturn(reportingYear);
             ReportFiguresHelper.SetFigures(viewModel, draftReturn, submittedReturn);

             return View("ReportFigures", viewModel);
        }

        private void PopulateViewModel(ReportFiguresViewModel viewModel, long organisationId, int reportingYear, bool hasDraftReturn)
         {
             Organisation organisation = dataRepository.Get<Organisation>(organisationId);

             viewModel.Organisation = organisation;
             viewModel.ReportingYear = reportingYear;
             viewModel.IsEditingSubmittedReturn = organisation.HasSubmittedReturn(reportingYear);
             viewModel.IsEditingForTheFirstTime = !viewModel.IsEditingSubmittedReturn && !hasDraftReturn;
             viewModel.SnapshotDate = organisation.SectorType.GetAccountingStartDate(reportingYear);
         }

        #endregion

        #region POST

        [HttpPost("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/figures")]
        [ValidateAntiForgeryToken]
        public IActionResult ReportFiguresPost(string encryptedOrganisationId, int reportingYear, ReportFiguresViewModel viewModel)
        {
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
                
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            ReportFiguresHelper.ValidateUserInput(viewModel, Request, reportingYear);

            if (viewModel.HasAnyErrors())
            {
                PopulateViewModel(viewModel, organisationId, reportingYear, !viewModel.IsEditingForTheFirstTime);
                return View("ReportFigures", viewModel);
            }

            SaveChangesToDraftReturn(viewModel, organisationId, reportingYear);
            
            var actionValues = new { encryptedOrganisationId, reportingYear, initialSubmission = viewModel.IsEditingForTheFirstTime };
            
            string personResponsibleUrl = Url.Action("ReportResponsiblePersonGet", "ReportResponsiblePerson", actionValues);
            string employerSizeUrl = Url.Action("ReportSizeOfOrganisationGet", "ReportSizeOfOrganisation", actionValues);
            string initialJourneyNextPageUrl = organisation.SectorType == SectorTypes.Private ? personResponsibleUrl : employerSizeUrl;
            string reportOverviewUrl = Url.Action("ReportOverview", "ReportOverview", actionValues);
            string nextPageUrl = viewModel.IsEditingForTheFirstTime ? initialJourneyNextPageUrl : reportOverviewUrl;

            StatusMessageHelper.SetStatusMessage(Response, "Saved changes to draft", nextPageUrl);
            return LocalRedirect(nextPageUrl);
        }

        private void SaveChangesToDraftReturn(ReportFiguresViewModel viewModel, long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = draftReturnService.GetOrCreateDraftReturn(organisationId, reportingYear);
            
            ReportFiguresHelper.SaveFiguresToDraftReturn(viewModel, draftReturn);
            
            draftReturnService.SaveDraftReturnOrDeleteIfNotRelevant(draftReturn);
        }
        

        #endregion
    }
}
