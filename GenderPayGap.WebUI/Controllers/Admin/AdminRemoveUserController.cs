using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
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
    public class AdminRemoveUserController : Controller
    {

        private readonly AuditLogger auditLogger;
        private readonly IDataRepository dataRepository;
        private readonly IRegistrationRepository RegistrationRepository;

        public AdminRemoveUserController(IDataRepository dataRepository,
            IRegistrationRepository registrationRepository,
            AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            RegistrationRepository = registrationRepository;
            this.auditLogger = auditLogger;
        }

        [HttpGet("/remove-user-organisation")]
        public IActionResult RemoveUserFromAnOrganisation(long orgId, long userId, bool fromViewUserPage)
        {
            UserOrganisation userOrg = dataRepository.GetAll<UserOrganisation>()
                .FirstOrDefault(u => u.UserId == userId && u.OrganisationId == orgId);

            ValidateUserOrganisationForRemoval(userOrg);

            var viewModel = new AdminRemoveUserViewModel
            {
                OrganisationId = userOrg.OrganisationId,
                OrganisationName = userOrg.Organisation.OrganisationName,
                UserId = userOrg.UserId,
                UserFullName = userOrg.User.Fullname,
                FromViewUserPage = fromViewUserPage
            };
            return View("ConfirmRemoving", viewModel);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("/remove-user-organisation")]
        public async Task<IActionResult> RemoveUserFromAnOrganisation(AdminRemoveUserViewModel viewModel)
        {
            var user = dataRepository.Get<User>(viewModel.UserId);
            var organisation = dataRepository.Get<Organisation>(viewModel.OrganisationId);
            UserOrganisation userOrg = dataRepository.GetAll<UserOrganisation>()
                .FirstOrDefault(u => u.UserId == viewModel.UserId && u.OrganisationId == viewModel.OrganisationId);
            ValidateUserOrganisationForRemoval(userOrg);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                // If there are any errors, return the user back to the same page to correct the mistakes
                return View("ConfirmRemoving", viewModel);
            }

            await RegistrationRepository.RemoveRegistrationAsync(userOrg, user);

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
            if (!user.EmailAddress.StartsWithI(Global.TestPrefix))
            {
                var sendEmails = new List<Task>();
                bool testEmail = !Config.IsProduction();
                if (organisation.GetIsOrphan())
                {
                    sendEmails.Add(Emails.SendGEOOrphanOrganisationNotificationAsync(organisation.OrganisationName, testEmail));
                }

                await Task.WhenAll(sendEmails);
            }

            // Audit log
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminRemoveUser,
                organisation,
                new {RemovedUserId = user.UserId, viewModel.Reason},
                currentUser);

            return View("SuccessfullyRemovedUser", viewModel);
        }

        private void ValidateUserOrganisationForRemoval(UserOrganisation userOrg)
        {
            if (userOrg == null)
            {
                throw new ArgumentException("There is no association between user and organisation.");
            }

            if (userOrg.PINConfirmedDate == null)
            {
                throw new ArgumentException(
                    $"User userId = {userOrg.UserId} is not fully registered with organistion organisationId = {userOrg.OrganisationId}");
            }
        }

    }
}
