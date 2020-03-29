using System;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Admin;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminOrganisationStatusController : Controller
    {

        private readonly AuditLogger auditLogger;

        private readonly IDataRepository dataRepository;

        public AdminOrganisationStatusController(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }

        [HttpGet("organisation/{id}/status")]
        public IActionResult ViewStatusHistory(long id)
        {
            var organisation = dataRepository.Get<Organisation>(id);

            return View("ViewOrganisationStatus", organisation);
        }


        [HttpGet("organisation/{id}/status/change")]
        public IActionResult ChangeStatusGet(long id)
        {
            var viewModel = new AdminChangeStatusViewModel();

            UpdateAdminChangeStatusViewModelFromOrganisation(viewModel, id);

            return View("ChangeStatus", viewModel);
        }

        [HttpPost("organisation/{id}/status/change")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatusPost(long id, AdminChangeStatusViewModel viewModel)
        {
            UpdateAdminChangeStatusViewModelFromOrganisation(viewModel, id);

            ValidateAdminChangeStatusViewModel(viewModel);

            if (viewModel.HasAnyErrors())
            {
                return View("ChangeStatus", viewModel);
            }

            if (viewModel.CurrentStatus == OrganisationStatuses.Active)
            {
                ChangeStatusFromActive(viewModel);
            }
            else if (viewModel.CurrentStatus == OrganisationStatuses.Retired)
            {
                ChangeStatusFromRetired(viewModel);
            }
            else
            {
                ChangeStatusFromDeleted(viewModel);
            }

            return RedirectToAction("ViewStatusHistory", "AdminOrganisationStatus", new {id});
        }

        private void UpdateAdminChangeStatusViewModelFromOrganisation(AdminChangeStatusViewModel viewModel, long organisationId)
        {
            var organisation = dataRepository.Get<Organisation>(organisationId);

            viewModel.OrganisationId = organisation.OrganisationId;
            viewModel.OrganisationName = organisation.OrganisationName;
            viewModel.CurrentStatus = organisation.Status;
        }

        private void ValidateAdminChangeStatusViewModel(AdminChangeStatusViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.CurrentStatus == OrganisationStatuses.Active)
            {
                viewModel.ParseAndValidateParameters(Request, m => m.NewStatusFromActive);
            }
            else if (viewModel.CurrentStatus == OrganisationStatuses.Retired)
            {
                viewModel.ParseAndValidateParameters(Request, m => m.NewStatusFromRetired);
            }
            else if (viewModel.CurrentStatus == OrganisationStatuses.Deleted)
            {
                viewModel.ParseAndValidateParameters(Request, m => m.NewStatusFromDeleted);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Organisation current status is not accepted");
            }
        }

        private void ChangeStatusFromActive(AdminChangeStatusViewModel viewModel)
        {
            if (viewModel.NewStatusFromActive == NewStatusesFromActive.Retired)
            {
                RetireOrganisation(viewModel);
            }
            else if (viewModel.NewStatusFromActive == NewStatusesFromActive.Deleted)
            {
                DeleteOrganisation(viewModel);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Organisation new status is not accepted");
            }
        }

        private void ChangeStatusFromRetired(AdminChangeStatusViewModel viewModel)
        {
            if (viewModel.NewStatusFromRetired == NewStatusesFromRetired.Active)
            {
                UnRetireOrganisation(viewModel);
            }
            else if (viewModel.NewStatusFromRetired == NewStatusesFromRetired.Deleted) { }
            else
            {
                throw new ArgumentOutOfRangeException("Organisation new status is not accepted");
            }
        }

        private void ChangeStatusFromDeleted(AdminChangeStatusViewModel viewModel)
        {
            if (viewModel.NewStatusFromDeleted == NewStatusesFromDeleted.Active)
            {
                UnDeleteOrganisation(viewModel);
            }
            else if (viewModel.NewStatusFromDeleted == NewStatusesFromDeleted.Retired) { }
            else
            {
                throw new ArgumentOutOfRangeException("Organisation new status is not accepted");
            }
        }

        private void RetireOrganisation(AdminChangeStatusViewModel viewModel) { }

        private void UnRetireOrganisation(AdminChangeStatusViewModel viewModel) { }

        private void DeleteOrganisation(AdminChangeStatusViewModel viewModel) { }

        private void UnDeleteOrganisation(AdminChangeStatusViewModel viwModel) { }

    }
}
