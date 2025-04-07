using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.RemoveUserFromOrganisation;
using GenderPayGap.WebUI.Repositories;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.ManageOrganisations
{
    [Route("account/organisations")]
    public class RemoveUserFromOrganisationController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly RegistrationRepository registrationRepository;
        private readonly EmailSendingService emailSendingService;

        public RemoveUserFromOrganisationController(
            IDataRepository dataRepository,
            RegistrationRepository registrationRepository,
            EmailSendingService emailSendingService)
        {
            this.dataRepository = dataRepository;
            this.registrationRepository = registrationRepository;
            this.emailSendingService = emailSendingService;
        }

        [HttpGet("{encryptedOrganisationId}/remove-user/{userToRemoveEncryptedUserId}")]
        [Authorize(Roles = LoginRoles.GpgEmployer)]
        public IActionResult RemoveUserFromOrganisationGet(string encryptedOrganisationId, string userToRemoveEncryptedUserId)
        {
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(user);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            var organisation = dataRepository.Get<Organisation>(organisationId);
            
            long userToRemoveUserId = ControllerHelper.DecryptUserIdOrThrow404(userToRemoveEncryptedUserId);
            User userToRemove = ControllerHelper.LoadUserOrThrow404(userToRemoveUserId, dataRepository);

            UserOrganisation userOrganisationToRemove = organisation.UserOrganisations.FirstOrDefault(uo => uo.UserId == userToRemove.UserId);

            if (userOrganisationToRemove == null)
            {
                // The user we're trying to remove isn't registered for this organisation
                throw new PageNotFoundException();
            }

            var viewModel = new RemoveUserFromOrganisationViewModel
            {
                UserToRemove = userToRemove,
                Organisation = organisation,
                LoggedInUser = user
            };

            return View("ConfirmRemove", viewModel);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("{encryptedOrganisationId}/remove-user/{userToRemoveEncryptedUserId}")]
        [Authorize(Roles = LoginRoles.GpgEmployer)]
        public IActionResult RemoveUserFromOrganisationPost(string encryptedOrganisationId, string userToRemoveEncryptedUserId)
        {
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(user);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            var organisation = dataRepository.Get<Organisation>(organisationId);
            
            long userToRemoveUserId = ControllerHelper.DecryptUserIdOrThrow404(userToRemoveEncryptedUserId);
            User userToRemove = ControllerHelper.LoadUserOrThrow404(userToRemoveUserId, dataRepository);

            UserOrganisation userOrganisationToRemove = organisation.UserOrganisations.FirstOrDefault(uo => uo.UserId == userToRemove.UserId);

            if (userOrganisationToRemove == null)
            {
                // The user we're trying to remove isn't registered for this organisation
                throw new PageNotFoundException();
            }

            RemoveUserFromOrganisation(userOrganisationToRemove, user);

            var viewModel = new RemoveUserFromOrganisationViewModel
            {
                UserToRemove = userToRemove,
                Organisation = organisation,
                LoggedInUser = user
            };

            return View("UserRemoved", viewModel);
        }

        private void RemoveUserFromOrganisation(UserOrganisation userOrganisationToRemove, User actionByUser)
        {
            User userToUnregister = userOrganisationToRemove.User;
            Organisation orgToRemove = userOrganisationToRemove.Organisation;
            
            registrationRepository.RemoveRegistration(userOrganisationToRemove, actionByUser);
            
            // Email user that has been unregistered
            if (!userToUnregister.HasBeenAnonymised)
            {
                emailSendingService.SendRemovedUserFromOrganisationEmail(
                    userToUnregister.EmailAddress,
                    orgToRemove.OrganisationName,
                    userToUnregister.Fullname);
            }

            // Email the other users of the organisation
            IEnumerable<string> emailAddressesForOrganisation = orgToRemove.UserOrganisations
                .Where(uo => !uo.User.HasBeenAnonymised)
                .Select(uo => uo.User.EmailAddress);
            
            foreach (string emailAddress in emailAddressesForOrganisation)
            {
                emailSendingService.SendRemovedUserFromOrganisationEmail(
                    emailAddress,
                    orgToRemove.OrganisationName,
                    userToUnregister.Fullname);
            }

            // Send the notification to GEO for each newly orphaned organisation
            if (orgToRemove.GetIsOrphan())
            {
                emailSendingService.SendGeoOrphanOrganisationEmail(orgToRemove.OrganisationName);
            }
        }

    }
}
