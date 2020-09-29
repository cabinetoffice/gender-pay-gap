﻿using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Account;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Account
{
    [Route("manage-account")]
    public class ChangeContactPreferencesController : Controller
    {

        private readonly IDataRepository dataRepository;

        public ChangeContactPreferencesController(
            IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }
        
        // The user asks to change their contact preferences
        // We direct them to a page that asks for their updated contact preferences
        [HttpGet("change-contact-preferences")]
        public IActionResult ChangeContactPreferencesGet()
        {
            // Get the current user
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            // Fill the viewModel with the current user's information
            var viewModel = new ChangeContactPreferencesViewModel
            {
                SendUpdates = currentUser.SendUpdates,
                AllowContact = currentUser.AllowContact
            };
            
            // Return the Change Contact Preferences form
            return View("ChangeContactPreferences", viewModel);
        }

        // The user submits some new contact preferences
        // We validate them
        // Then we save the updates and return the user to the Manage Account page
        [HttpPost("change-contact-preferences")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeContactPreferencesPost(ChangeContactPreferencesViewModel viewModel)
        {
            // Get the user db entry
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            
            // Update the user's information
            currentUser.SendUpdates = viewModel.SendUpdates;
            currentUser.AllowContact = viewModel.AllowContact;

            // Save updates
            dataRepository.SaveChanges();
            
            string nextPageUrl = Url.Action("ManageAccount", "ManageAccount", new {Area = "Account"});
            StatusMessageHelper.SetStatusMessage(Response, "Saved changes to contact preferences", nextPageUrl);

            // Return user to the Manage Account page
            return LocalRedirect(nextPageUrl);
        }

    }
}
