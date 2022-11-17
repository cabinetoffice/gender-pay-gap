using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.ReportStarting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Report
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("account/organisations")]
    public class ReportStartingController: Controller
    {
        private readonly IDataRepository dataRepository;
        public ReportStartingController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }
        
        #region GET
        
        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/start")]
        public IActionResult ReportingStart(string encryptedOrganisationId, int reportingYear)
        {
             long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
             
             ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
             ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
             ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

             var viewModel = new ReportStartingViewModel();
             
             PopulateViewModel(viewModel, organisationId, reportingYear);

             return View("ReportStarting", viewModel);
        }

        private void PopulateViewModel(ReportStartingViewModel viewModel, long organisationId, int reportingYear)
         {
             Organisation organisation = dataRepository.Get<Organisation>(organisationId);

             viewModel.Organisation = organisation;
             viewModel.ReportingYear = reportingYear;
             viewModel.SnapshotDate = organisation.SectorType.GetAccountingStartDate(reportingYear);
         }

        #endregion
    }
}
