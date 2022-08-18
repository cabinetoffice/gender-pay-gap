using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Account;
using GenderPayGap.WebUI.Repositories;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Account
{
    [Route("manage-account")]
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    public class CloseAccountController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly IUserRepository userRepository;
        private readonly RegistrationRepository registrationRepository;
        private readonly EmailSendingService emailSendingService;

        public CloseAccountController(
            IDataRepository dataRepository, 
            IUserRepository userRepository, 
            RegistrationRepository registrationRepository,
            EmailSendingService emailSendingService)
        {
            this.dataRepository = dataRepository;
            this.userRepository = userRepository;
            this.registrationRepository = registrationRepository;
            this.emailSendingService = emailSendingService;
        }

        // The user asks to close their account
        // We direct them to a page that asks for confirmation
        [HttpGet("close")]
        public IActionResult CloseAccountGet()
        {
            // Admin impersonating a user shouldn't be able to close the user's account
            if (LoginHelper.IsUserBeingImpersonated(User))
            {
                return RedirectToAction("ManageAccountGet", "ManageAccount");
            }
            
            // Get the current user
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            
            var viewModel = new CloseAccountViewModel
            {
                IsSoleUserRegisteredToAnOrganisation = currentUser.IsSoleUserOfOneOrMoreOrganisations()
            };

            // Return the Change Personal Details form
            return View("CloseAccount", viewModel);
        }

        [HttpPost("close")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult CloseAccountPost(CloseAccountViewModel viewModel)
        {
            // Admin impersonating a user shouldn't be able to close the user's account
            if (LoginHelper.IsUserBeingImpersonated(User))
            {
                return RedirectToAction("ManageAccountGet", "ManageAccount");
            }
            
            if (!ModelState.IsValid)
            {
                return View("CloseAccount", viewModel);
            }
            
            // Get the current user
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            
            // Check password
            bool isValidPassword = userRepository.CheckPassword(currentUser, viewModel.Password);
            if (!isValidPassword)
            {
                ModelState.AddModelError(nameof(viewModel.Password), "Could not verify your password");
                return View("CloseAccount", viewModel);
            }

            // Collect list of organisations associated with the user
            List<Organisation> possiblyOrphanedOrganisations = currentUser.UserOrganisations
                .Select(uo => uo.Organisation)
                .Distinct()
                .ToList();

            RetireUserAccount(currentUser);

            SendAccountClosedEmail(currentUser);

            // Collect list of orphaned organisations after the user has been retired
            List<Organisation> orphanedOrganisations = possiblyOrphanedOrganisations.FindAll(org => org.GetIsOrphan());

            InformGeoOfOrphanedOrganisations(orphanedOrganisations);
            
            // Log user out and redirect to success page
            IActionResult accountClosedSuccess = RedirectToAction("CloseAccountComplete", "CloseAccount");

            return LoginHelper.Logout(HttpContext, accountClosedSuccess);
        }
        
        // The user asks to close their account
        // We direct them to a page that asks for confirmation
        [AllowAnonymous]
        [HttpGet("close/completed")]
        public IActionResult CloseAccountComplete()
        {
            // Return the Close Account completed page
            return View("CloseAccountComplete");
        }

        private void RetireUserAccount(User user)
        {
            try
            {
                // update retired user registrations 
                registrationRepository.RemoveRetiredUserRegistrations(user);

                // retire user
                userRepository.RetireUser(user);
            }
            catch (Exception ex)
            {
                CustomLogger.Warning($"Failed to retire user {user.UserId}", ex);
                throw;
            }
        }

        private void SendAccountClosedEmail(User user)
        {
            // Send email to user informing them of account closure
            emailSendingService.SendCloseAccountCompletedEmail(user.EmailAddress);
        }

        private void InformGeoOfOrphanedOrganisations(List<Organisation> orphanedOrganisations)
        {
            // Email GEO for each newly orphaned organisation
            orphanedOrganisations
                .ForEach(
                    org => emailSendingService.SendGeoOrphanOrganisationEmail(org.OrganisationName));
        }

    }
}
