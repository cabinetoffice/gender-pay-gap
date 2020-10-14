﻿using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Account;
using GenderPayGap.WebUI.Services;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Account
{
    [Route("manage-account")]
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    public class ChangePersonalDetailsController : Controller
    {
        
        private readonly IDataRepository dataRepository;
        private readonly AuditLogger auditLogger;

        public ChangePersonalDetailsController(
            IDataRepository dataRepository,
            AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }

        // The user asks to change their personal details
        // We direct them to a page that asks for their updated personal details
        [HttpGet("change-personal-details")]
        public IActionResult ChangePersonalDetailsGet()
        {
            ControllerHelper.ThrowIfAdminIsImpersonatingUser(User);

            // Get the current user
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            // Fill the viewModel with the current user's information
            var viewModel = new ChangePersonalDetailsViewModel
            {
                FirstName = currentUser.Firstname,
                LastName = currentUser.Lastname,
                JobTitle = currentUser.JobTitle,
                ContactPhoneNumber = currentUser.ContactPhoneNumber
            };

            // Return the Change Personal Details form
            return View("ChangePersonalDetails", viewModel);
        }
        
        // The user submits some new personal details
        // We validate it (e.g. that all fields except contact phone number are filled in)
        // Then we save the updates and return the user to the Manage Account page
        [HttpPost("change-personal-details")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePersonalDetailsPost(ChangePersonalDetailsViewModel viewModel)
        {
            ControllerHelper.ThrowIfAdminIsImpersonatingUser(User);

            viewModel.ParseAndValidateParameters(Request, m => m.FirstName);
            viewModel.ParseAndValidateParameters(Request, m => m.LastName);
            viewModel.ParseAndValidateParameters(Request, m => m.JobTitle);
            viewModel.ParseAndValidateParameters(Request, m => m.ContactPhoneNumber);

            if (viewModel.HasAnyErrors())
            {
                return View("ChangePersonalDetails", viewModel);
            }

            // Get the user db entry
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            
            AuditLogChanges(currentUser, viewModel);
            SaveUserDetails(currentUser, viewModel);

            string nextPageUrl = Url.Action("ManageAccountGet", "ManageAccount");
            StatusMessageHelper.SetStatusMessage(Response, "Saved changes to personal details", nextPageUrl);

            // Return user to the Manage Account page
            return LocalRedirect(nextPageUrl);
        }

        private void AuditLogChanges(User user, ChangePersonalDetailsViewModel viewModel)
        {
            if (user.Firstname != viewModel.FirstName ||
               user.Lastname != viewModel.LastName ||
               user.ContactFirstName != viewModel.FirstName ||
               user.ContactLastName != viewModel.LastName)
            {
                auditLogger.AuditChangeToUser(
                    AuditedAction.UserChangeName,
                    user,
                    new
                    {
                        OldFirstName = user.Firstname,
                        OldLastName = user.Lastname,
                        OldContactFirstName = user.ContactFirstName,
                        OldContactLastName = user.ContactLastName,
                        NewFirstName = viewModel.FirstName,
                        NewLastName = viewModel.LastName,
                    },
                    user);
            }

            if (user.JobTitle != viewModel.JobTitle ||
                user.ContactJobTitle != viewModel.JobTitle)
            {
                auditLogger.AuditChangeToUser(
                    AuditedAction.UserChangeJobTitle,
                    user,
                    new
                    {
                        OldJobTitle = user.JobTitle,
                        OldContactJobTitle = user.ContactJobTitle,
                        NewJobTitle = viewModel.JobTitle,
                    },
                    user);
            }

            if (user.ContactPhoneNumber != viewModel.ContactPhoneNumber)
            {
                auditLogger.AuditChangeToUser(
                    AuditedAction.UserChangePhoneNumber,
                    user,
                    new
                    {
                        OldContactPhoneNumber = user.ContactPhoneNumber,
                        NewContactPhoneNumber = viewModel.ContactPhoneNumber,
                    },
                    user);
            }
        }

        private void SaveUserDetails(User currentUser, ChangePersonalDetailsViewModel viewModel)
        {
            // Update the user's information
            currentUser.Firstname = viewModel.FirstName;
            currentUser.ContactFirstName = viewModel.FirstName;

            currentUser.Lastname = viewModel.LastName;
            currentUser.ContactLastName = viewModel.LastName;

            currentUser.JobTitle = viewModel.JobTitle;
            currentUser.ContactJobTitle = viewModel.JobTitle;

            currentUser.ContactPhoneNumber = viewModel.ContactPhoneNumber;

            currentUser.Modified = VirtualDateTime.Now;

            // Save updates
            dataRepository.SaveChanges();
        }

    }
}
