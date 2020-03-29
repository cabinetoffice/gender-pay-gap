﻿using System;
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

        private void RetireOrganisation(AdminChangeStatusViewModel viewModel)
        {
            var organisation = dataRepository.Get<Organisation>(viewModel.OrganisationId);
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            organisation.SetStatus(
                OrganisationStatuses.Retired,
                currentUser.UserId,
                viewModel.Reason);

            InactivateUsersOfOrganisation(organisation);

            dataRepository.SaveChangesAsync().Wait();

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangeOrganisationStatus,
                organisation,
                new {PreviousStatus = OrganisationStatuses.Active, NewStatus = OrganisationStatuses.Retired, viewModel.Reason},
                currentUser);

            // No need to update search index because a retired organisation is similar to an active organisation in terms of search
        }

        private void UnRetireOrganisation(AdminChangeStatusViewModel viewModel)
        {
            var organisation = dataRepository.Get<Organisation>(viewModel.OrganisationId);
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            organisation.SetStatus(
                OrganisationStatuses.Active,
                currentUser.UserId,
                viewModel.Reason);

            ActivateUsersOfOrganisation(organisation);

            dataRepository.SaveChangesAsync().Wait();

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangeOrganisationStatus,
                organisation,
                new {PreviousStatus = OrganisationStatuses.Retired, NewStatus = OrganisationStatuses.Active, viewModel.Reason},
                currentUser);

            // No need to update search index because a retired organisation is similar to an active organisation in terms of search
        }

        private void DeleteOrganisation(AdminChangeStatusViewModel viewModel)
        {
            var organisation = dataRepository.Get<Organisation>(viewModel.OrganisationId);
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            organisation.SetStatus(
                OrganisationStatuses.Deleted,
                currentUser.UserId,
                viewModel.Reason);

            InactivateUsersOfOrganisation(organisation);

            dataRepository.SaveChangesAsync().Wait();

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangeOrganisationStatus,
                organisation,
                new {PreviousStatus = OrganisationStatuses.Active, NewStatus = OrganisationStatuses.Deleted, viewModel.Reason},
                currentUser);

            // Remove this organisation from the search index
            searchBusinessLogic.UpdateSearchIndexAsync(organisation).Wait();
        }

        private void UnDeleteOrganisation(AdminChangeStatusViewModel viewModel)
        {
            var organisation = dataRepository.Get<Organisation>(viewModel.OrganisationId);
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            organisation.SetStatus(
                OrganisationStatuses.Active,
                currentUser.UserId,
                viewModel.Reason);

            ActivateUsersOfOrganisation(organisation);

            dataRepository.SaveChangesAsync().Wait();

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangeOrganisationStatus,
                organisation,
                new {PreviousStatus = OrganisationStatuses.Deleted, NewStatus = OrganisationStatuses.Active, viewModel.Reason},
                currentUser);

            // Add this organisation to the search index
            searchBusinessLogic.UpdateSearchIndexAsync(organisation).Wait();
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
