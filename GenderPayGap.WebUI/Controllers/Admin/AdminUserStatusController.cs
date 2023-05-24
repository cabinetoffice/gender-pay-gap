using System;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Services;
using GovUkDesignSystem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminUserStatusController : Controller
    {

        private readonly AuditLogger auditLogger;

        private readonly IDataRepository dataRepository;

        public AdminUserStatusController(IDataRepository dataRepository,
            AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }

        [HttpGet("user/{id}/status")]
        public IActionResult ViewStatusHistory(long id)
        {
            var user = dataRepository.Get<User>(id);

            return View("ViewUserStatus", user);
        }


        [HttpGet("user/{id}/status/change")]
        public IActionResult ChangeStatusGet(long id)
        {
            var viewModel = new AdminChangeUserStatusViewModel();

            UpdateAdminChangeStatusViewModelFromUser(viewModel, id);

            return View("ChangeStatus", viewModel);
        }

        [HttpPost("user/{id}/status/change")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeStatusPost(long id, AdminChangeUserStatusViewModel viewModel)
        {
            switch (viewModel.Action)
            {
                case ChangeUserStatusViewModelActions.OfferNewStatusAndReason:
                    UpdateAdminChangeStatusViewModelFromUser(viewModel, id);
                    if (!ModelState.IsValid)
                    {
                        return View("ChangeStatus", viewModel);
                    }

                    return View("ConfirmStatusChange", viewModel);

                case ChangeUserStatusViewModelActions.ConfirmStatusChange:
                    ChangeStatus(viewModel, id);
                    return RedirectToAction("ViewStatusHistory", "AdminUserStatus", new {id});
                default:
                    throw new ArgumentException("Unknown action in AdminUserStatusController.ChangeStatusPost");
            }
        }

        private void UpdateAdminChangeStatusViewModelFromUser(AdminChangeUserStatusViewModel viewModel, long userId)
        {
            viewModel.User = dataRepository.Get<User>(userId);

            viewModel.InactiveUserOrganisations = dataRepository.GetAll<InactiveUserOrganisation>()
                .Where(m => m.UserId == userId).ToList();
        }

        private void ChangeStatus(AdminChangeUserStatusViewModel viewModel, long userId)
        {
            var user = dataRepository.Get<User>(userId);
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            UserStatuses previousStatus = user.Status;
            UserStatuses newStatus = viewModel.NewStatus ?? UserStatuses.Unknown;

            // Update the status
            user.SetStatus(
                newStatus,
                currentUser,
                viewModel.Reason);

            // [In]Activate organisations of the user
            if (newStatus == UserStatuses.Active)
            {
                ActivateOrganisationsOfUser(user);
            }

            if (previousStatus == UserStatuses.Active)
            {
                InactivateOrganisationsOfUser(user);
            }

            dataRepository.SaveChanges();

            // Audit log
            auditLogger.AuditChangeToUser(
                AuditedAction.AdminChangeUserStatus,
                user,
                new {PreviousStatus = previousStatus, NewStatus = newStatus, viewModel.Reason},
                User);

        }

        private void ActivateOrganisationsOfUser(User user)
        {
            IQueryable<InactiveUserOrganisation> inactiveUserOrganisations = dataRepository.GetAll<InactiveUserOrganisation>()
                .Where(m => m.UserId == user.UserId);
            foreach (InactiveUserOrganisation inactiveUserOrganisation in inactiveUserOrganisations)
            {
                user.UserOrganisations.Add(CreateUserOrganisationFromInactiveUserOrganisation(inactiveUserOrganisation));
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
                Method = inactiveUserOrganisation.Method,
                Created = inactiveUserOrganisation.Created
            };
        }

        private void InactivateOrganisationsOfUser(User user)
        {
            foreach (UserOrganisation userOrganisation in user.UserOrganisations)
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
                Method = userOrganisation.Method,
                Created = userOrganisation.Created
            };
        }

    }
}
