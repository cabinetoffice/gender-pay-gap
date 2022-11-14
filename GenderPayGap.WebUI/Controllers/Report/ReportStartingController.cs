using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Report;
using GenderPayGap.WebUI.Models.ReportStarting;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Report
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("account/organisations")]
    public class ReportStartingController: Controller
    {
        
        private readonly IDataRepository dataRepository;
        private readonly DraftReturnService draftReturnService;

        public ReportStartingController(
            IDataRepository dataRepository,
            DraftReturnService draftReturnService)
        {
            this.dataRepository = dataRepository;
            this.draftReturnService = draftReturnService;
        }
        
        #region GET
        
        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/start")]
        public IActionResult ReportFiguresGet(string encryptedOrganisationId, int reportingYear)
        {
             long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
             
             ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
             ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
             ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

             var viewModel = new ReportStartingViewModel();

             DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
             PopulateViewModel(viewModel, organisationId, reportingYear, draftReturn != null);

             //Return submittedReturn = viewModel.Organisation.GetReturn(reportingYear);
             //ReportFiguresHelper.SetFigures(viewModel, draftReturn, submittedReturn);

             return View("ReportStarting", viewModel);
        }

        private void PopulateViewModel(ReportStartingViewModel viewModel, long organisationId, int reportingYear, bool hasDraftReturn)
         {
             Organisation organisation = dataRepository.Get<Organisation>(organisationId);

             viewModel.Organisation = organisation;
             viewModel.ReportingYear = reportingYear;
             viewModel.IsEditingSubmittedReturn = organisation.HasSubmittedReturn(reportingYear);
             viewModel.IsEditingForTheFirstTime = !viewModel.IsEditingSubmittedReturn && !hasDraftReturn;
             viewModel.SnapshotDate = organisation.SectorType.GetAccountingStartDate(reportingYear);
         }

        #endregion
    }
}
