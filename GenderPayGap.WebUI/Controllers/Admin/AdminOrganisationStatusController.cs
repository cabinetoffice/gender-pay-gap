using System;
using System.Linq;
using GenderPayGap.BusinessLogic;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GovUkDesignSystem;
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

        public AdminOrganisationStatusController(IDataRepository dataRepository,
            AuditLogger auditLogger)
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
        public IActionResult ChangeStatusPost(long id, AdminChangeStatusViewModel viewModel)
        {
            switch (viewModel.Action)
            {
                case ChangeOrganisationStatusViewModelActions.OfferNewStatusAndReason:
                    UpdateAdminChangeStatusViewModelFromOrganisation(viewModel, id);
                    ValidateAdminChangeStatusViewModel(viewModel);
                    if (viewModel.HasAnyErrors())
                    {
                        return View("ChangeStatus", viewModel);
                    }

                    return View("ConfirmStatusChange", viewModel);

                case ChangeOrganisationStatusViewModelActions.ConfirmStatusChange:
                    ChangeStatus(viewModel, id);
                    return RedirectToAction("ViewStatusHistory", "AdminOrganisationStatus", new {id});
                default:
                    throw new ArgumentException("Unknown action in AdminOrganisationStatusController.ChangeStatusPost");
            }
        }

        private void UpdateAdminChangeStatusViewModelFromOrganisation(AdminChangeStatusViewModel viewModel, long organisationId)
        {
            viewModel.Organisation = dataRepository.Get<Organisation>(organisationId);

            viewModel.InactiveUserOrganisations = dataRepository.GetAll<InactiveUserOrganisation>()
                .Where(m => m.OrganisationId == organisationId).ToList();
        }

        private void ValidateAdminChangeStatusViewModel(AdminChangeStatusViewModel viewModel)
        {
            if (!viewModel.NewStatus.HasValue)
            {
                viewModel.AddErrorFor(m => m.NewStatus, "Please select a new status");
            }

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);
        }

        private void ChangeStatus(AdminChangeStatusViewModel viewModel, long organisationId)
        {
            var organisation = dataRepository.Get<Organisation>(organisationId);
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            OrganisationStatuses previousStatus = organisation.Status;
            OrganisationStatuses newStatus = viewModel.NewStatus ?? OrganisationStatuses.Unknown;

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
            
        }

        private void ActivateUsersOfOrganisation(Organisation organisation)
        {
            IQueryable<InactiveUserOrganisation> inactiveUserOrganisations = dataRepository.GetAll<InactiveUserOrganisation>()
                .Where(m => m.OrganisationId == organisation.OrganisationId);
            foreach (InactiveUserOrganisation inactiveUserOrganisation in inactiveUserOrganisations)
            {
                organisation.UserOrganisations.Add(CreateUserOrganisationFromInactiveUserOrganisation(inactiveUserOrganisation));
                dataRepository.Delete(inactiveUserOrganisation);
            }
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

        private void InactivateUsersOfOrganisation(Organisation organisation)
        {
            foreach (UserOrganisation userOrganisation in organisation.UserOrganisations)
            {
                dataRepository.Insert(CreateInactiveUserOrganisationFromUserOrganisation(userOrganisation));
                dataRepository.Delete(userOrganisation);
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

    }
}
