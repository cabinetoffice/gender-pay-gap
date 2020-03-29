using System;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
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

        private readonly ISearchBusinessLogic searchBusinessLogic;

        public AdminOrganisationStatusController(IDataRepository dataRepository,
            AuditLogger auditLogger,
            ISearchBusinessLogic searchBusinessLogic)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
            this.searchBusinessLogic = searchBusinessLogic;
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

            var organisation = dataRepository.Get<Organisation>(viewModel.OrganisationId);
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            OrganisationStatuses previousStatus = organisation.Status;
            OrganisationStatuses newStatus = FindNewStatus(organisation, viewModel);

            // Update the status
            organisation.SetStatus(
                newStatus,
                currentUser.UserId,
                viewModel.Reason);

            // [In]Activate users of the organisaton
            if (newStatus == OrganisationStatuses.Active)
            {
                ActivateUsersOfOrganisation(organisation);
            }

            if (previousStatus == OrganisationStatuses.Active)
            {
                InactivateUsersOfOrganisation(organisation);
            }

            dataRepository.SaveChangesAsync().Wait();

            // Audit log
            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangeOrganisationStatus,
                organisation,
                new {PreviousStatus = previousStatus, NewStatus = newStatus, viewModel.Reason},
                currentUser);

            // Add/remove this organisation to/from the search index when status has bee changed from/to Deleted
            if (previousStatus == OrganisationStatuses.Deleted || newStatus == OrganisationStatuses.Deleted)
            {
                searchBusinessLogic.UpdateSearchIndexAsync(organisation).Wait();
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

        private OrganisationStatuses FindNewStatus(Organisation organisation, AdminChangeStatusViewModel viewModel)
        {
            switch (organisation.Status)
            {
                case OrganisationStatuses.Active:
                    return viewModel.NewStatusFromActive == NewStatusesFromActive.Retired
                        ? OrganisationStatuses.Retired
                        : OrganisationStatuses.Deleted;
                case OrganisationStatuses.Retired:
                    return viewModel.NewStatusFromRetired == NewStatusesFromRetired.Active
                        ? OrganisationStatuses.Active
                        : OrganisationStatuses.Deleted;
                case OrganisationStatuses.Deleted:
                    return viewModel.NewStatusFromDeleted == NewStatusesFromDeleted.Active
                        ? OrganisationStatuses.Active
                        : OrganisationStatuses.Retired;
                default:
                    throw new ArgumentException("Organisation current status is not acceptable for this action.");
            }
        }

        private void InactivateUsersOfOrganisation(Organisation organisation)
        {
            foreach (UserOrganisation userOrganisation in organisation.UserOrganisations)
            {
                organisation.InactiveUserOrganisations.Add(CreateInactiveUserOrganisationFromUserOrganisation(userOrganisation));
                dataRepository.Delete(userOrganisation);
            }
        }

        private void ActivateUsersOfOrganisation(Organisation organisation)
        {
            foreach (InactiveUserOrganisation inactiveUserOrganisation in organisation.InactiveUserOrganisations)
            {
                organisation.UserOrganisations.Add(CreateUserOrganisationFromInactiveUserOrganisation(inactiveUserOrganisation));
                dataRepository.Delete(inactiveUserOrganisation);
            }
        }

        private InactiveUserOrganisation CreateInactiveUserOrganisationFromUserOrganisation(UserOrganisation userOrganisation)
        {
            return new InactiveUserOrganisation
            {
                UserId = userOrganisation.UserId,
                OrganisationId = userOrganisation.OrganisationId,
                PIN = userOrganisation.PIN,
                PINSentDate = userOrganisation.PINSentDate,
                PITPNotifyLetterId = userOrganisation.PITPNotifyLetterId,
                PINConfirmedDate = userOrganisation.PINConfirmedDate,
                ConfirmAttemptDate = userOrganisation.ConfirmAttemptDate,
                ConfirmAttempts = userOrganisation.ConfirmAttempts,
                AddressId = userOrganisation.AddressId,
                Method = userOrganisation.Method
            };
        }

        private UserOrganisation CreateUserOrganisationFromInactiveUserOrganisation(InactiveUserOrganisation inactiveUserOrganisation)
        {
            return new UserOrganisation
            {
                UserId = inactiveUserOrganisation.UserId,
                OrganisationId = inactiveUserOrganisation.OrganisationId,
                PIN = inactiveUserOrganisation.PIN,
                PINSentDate = inactiveUserOrganisation.PINSentDate,
                PITPNotifyLetterId = inactiveUserOrganisation.PITPNotifyLetterId,
                PINConfirmedDate = inactiveUserOrganisation.PINConfirmedDate,
                ConfirmAttemptDate = inactiveUserOrganisation.ConfirmAttemptDate,
                ConfirmAttempts = inactiveUserOrganisation.ConfirmAttempts,
                AddressId = inactiveUserOrganisation.AddressId,
                Method = inactiveUserOrganisation.Method
            };
        }

    }
}
