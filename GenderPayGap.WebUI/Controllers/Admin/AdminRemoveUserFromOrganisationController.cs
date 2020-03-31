using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Services;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminRemoveUserFromOrganisationController : Controller
    {

        private readonly AuditLogger auditLogger;
        private readonly IDataRepository dataRepository;

        public AdminRemoveUserFromOrganisationController(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }

        // When coming from the Organisation page
        [HttpGet("organisation/{organisationId}/remove-user/{userId}")]
        public IActionResult RemoveUserFromOrganisationGet(long organisationId, long userId)
        {
            return UnlinkUserAndOrganisationGet(organisationId, userId, false);
        }

        // When coming from the User page
        [HttpGet("user/{userId}/remove-organisation/{organisationId}")]
        public IActionResult RemoveOrganisationFromUserGet(long organisationId, long userId)
        {
            return UnlinkUserAndOrganisationGet(organisationId, userId, true);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("organisation/{organisationId}/remove-user/{userId}")]
        public async Task<IActionResult> RemoveUserFromOrganisationPost(
            long organisationId,
            long userId,
            AdminRemoveUserViewModel viewModel)
        {
            return await UnlinkUserAndOrganisationPost(organisationId, userId, viewModel, false);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("user/{userId}/remove-organisation/{organisationId}")]
        public async Task<IActionResult> RemoveOrganisationFromUserPost(
            long organisationId,
            long userId,
            AdminRemoveUserViewModel viewModel)
        {
            return await UnlinkUserAndOrganisationPost(organisationId, userId, viewModel, true);
        }

        private IActionResult UnlinkUserAndOrganisationGet(long organisationId, long userId, bool fromViewUserPage)
        {
            var viewModel = new AdminRemoveUserViewModel();

            UpdateAdminRemoveUserViewModelFromUserOrganisation(viewModel, organisationId, userId);
            viewModel.FromViewUserPage = fromViewUserPage;

            return View("ConfirmRemoving", viewModel);
        }

        private void UpdateAdminRemoveUserViewModelFromUserOrganisation(
            AdminRemoveUserViewModel viewModel,
            long organisationId,
            long userId)
        {
            UserOrganisation userOrg = dataRepository.GetAll<UserOrganisation>()
                .FirstOrDefault(u => u.UserId == userId && u.OrganisationId == organisationId);

            ValidateUserOrganisationForRemoval(userOrg);

            viewModel.OrganisationId = userOrg.OrganisationId;
            viewModel.OrganisationName = userOrg.Organisation.OrganisationName;
            viewModel.UserId = userOrg.UserId;
            viewModel.UserFullName = userOrg.User.Fullname;
        }

        private async Task<IActionResult> UnlinkUserAndOrganisationPost(
            long organisationId,
            long userId,
            AdminRemoveUserViewModel viewModel,
            bool fromViewUserPage)
        {
            UpdateAdminRemoveUserViewModelFromUserOrganisation(viewModel, organisationId, userId);
            viewModel.FromViewUserPage = fromViewUserPage;

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                // If there are any errors, return the user back to the same page to correct the mistakes
                return View("ConfirmRemoving", viewModel);
            }

            var user = dataRepository.Get<User>(viewModel.UserId);
            var organisation = dataRepository.Get<Organisation>(viewModel.OrganisationId);

            // Remove user organisation 
            organisation.LatestRegistration = null;
            UserOrganisation userOrg = dataRepository.GetAll<UserOrganisation>()
                .FirstOrDefault(u => u.UserId == viewModel.UserId && u.OrganisationId == viewModel.OrganisationId);
            dataRepository.Delete(userOrg);

            await dataRepository.SaveChangesAsync();

            // Email user that has been unregistered
            EmailSendingService.SendRemovedUserFromOrganisationEmail(
                user.EmailAddress,
                organisation.OrganisationName,
                user.Fullname);

            // Email the other users of the organisation
            IEnumerable<string> emailAddressesForOrganisation = organisation.UserOrganisations.Select(uo => uo.User.EmailAddress);
            foreach (string emailAddress in emailAddressesForOrganisation)
            {
                EmailSendingService.SendRemovedUserFromOrganisationEmail(
                    emailAddress,
                    organisation.OrganisationName,
                    user.Fullname);
            }

            // Send the notification to GEO for each newly orphaned organisation
            if (organisation.GetIsOrphan())
            {
                bool testEmail = !Config.IsProduction();
                await Emails.SendGEOOrphanOrganisationNotificationAsync(organisation.OrganisationName, testEmail);
            }

            // Audit log
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminRemoveUserFromOrganisation,
                organisation,
                new {RemovedUserId = user.UserId, viewModel.Reason},
                currentUser);

            return View("SuccessfullyRemoved", viewModel);
        }

        private void ValidateUserOrganisationForRemoval(UserOrganisation userOrg)
        {
            if (userOrg == null)
            {
                throw new ArgumentException("There is no association between user and organisation.");
            }
        }

    }
}
