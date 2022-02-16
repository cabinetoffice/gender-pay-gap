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
    [Route("account/employers")]
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
             
             ReportFiguresHelper.SetFigures(viewModel, draftReturn, submittedReturn);

             return View("ReportFigures", viewModel);
        }

        private void PopulateViewModel(ReportFiguresViewModel viewModel, long organisationId, int reportingYear)
         {
             Organisation organisation = dataRepository.Get<Organisation>(organisationId);

             viewModel.Organisation = organisation;
             viewModel.ReportingYear = reportingYear;
             viewModel.IsEditingSubmittedReturn = ReportHelper.HasSubmittedReturn(organisation, reportingYear);
             viewModel.SnapshotDate = organisation.SectorType.GetAccountingStartDate(reportingYear);
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

            ReportFiguresHelper.ValidateUserInput(viewModel, Request, reportingYear);

            if (viewModel.HasAnyErrors())
            {
                PopulateViewModel(viewModel, organisationId, reportingYear);
                return View("ReportFigures", viewModel);
            }

            SaveChangesToDraftReturn(viewModel, organisationId, reportingYear);

            string nextPageUrl = Url.Action("ReportOverview", "ReportOverview", new { encryptedOrganisationId, reportingYear});
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
