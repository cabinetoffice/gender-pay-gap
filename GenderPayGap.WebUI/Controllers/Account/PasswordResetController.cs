﻿using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Account;
using GenderPayGap.WebUI.Repositories;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Account
{
    public class PasswordResetController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly UserRepository userRepository;
        private readonly EmailSendingService emailSendingService;

        public PasswordResetController(
            IDataRepository dataRepository,
            UserRepository userRepository,
            EmailSendingService emailSendingService)
        {
            this.dataRepository = dataRepository;
            this.userRepository = userRepository;
            this.emailSendingService = emailSendingService;
        }

        #region Send password reset email

        [HttpGet("password-reset")]
        public IActionResult PasswordResetGet()
        {
            // Redirect if already logged in
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
            }
            
            return View("PasswordReset", new PasswordResetViewModel());
        }

        [ValidateAntiForgeryToken]
        [HttpPost("password-reset")]
        public IActionResult PasswordResetPost(PasswordResetViewModel viewModel)
        {
            // Redirect if already logged in
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
            }
            
            if (!ModelState.IsValid)
            {
                return View("PasswordReset", viewModel); 
            }

            // Find user associated with email address
            User userForPasswordReset = userRepository.FindByEmail(viewModel.EmailAddress, UserStatuses.Active);

            if (userForPasswordReset == null)
            {
                ModelState.AddModelError(nameof(viewModel.EmailAddress), "An account associated with this email address does not exist.");
                
                return View("PasswordReset", viewModel); 
            }

            // If password reset email has been sent (and password hasn't been changed) within the last 10 minutes, show an error page
            if (PasswordResetEmailSentTooRecently(userForPasswordReset))
            {
                throw new UserRecentlySentPasswordResetEmailWithoutChangingPasswordException();
            }
            
            SendPasswordResetEmail(userForPasswordReset);

            var passwordResetSentViewModel = new PasswordResetSentViewModel {EmailAddress = viewModel.EmailAddress};

            return View("PasswordResetSent", passwordResetSentViewModel);
        }

        private static bool PasswordResetEmailSentTooRecently(User userForPasswordReset)
        {
            return userForPasswordReset.ResetSendDate.HasValue
                   && (VirtualDateTime.Now - userForPasswordReset.ResetSendDate.Value).TotalMinutes < Global.MinPasswordResetMinutes;
        }

        // Generates and stores a string to act as a password reset code
        private void SendPasswordResetEmail(User userForPasswordReset)
        {
            // Generate a random string as a unique identifier for the password reset
            string resetCode = Convert.ToBase64String(PasswordHelper.GetSalt());

            // Store the reset code on the user entity for verification
            userForPasswordReset.PasswordResetCode = resetCode;
            userForPasswordReset.ResetSendDate = VirtualDateTime.Now;
            dataRepository.SaveChanges();

            string resetUrl = Url.Action("ChooseNewPasswordGet", "PasswordReset", new { code = resetCode }, "https");
            emailSendingService.SendResetPasswordVerificationEmail(userForPasswordReset.EmailAddress, resetUrl);

            CustomLogger.Information(
                "Password reset sent",
                $"User ID: {userForPasswordReset.UserId}, Email:{userForPasswordReset.EmailAddress}");
        }

        #endregion

        #region Set new password

        [HttpGet("choose-new-password")]
        public IActionResult ChooseNewPasswordGet(string code)
        {
            // Redirect if already logged in
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
            }

            // Find the user from the reset code in the viewModel
            User user = GetUserFromResetCode(code);

            // Check that password reset code has not expired
            ThrowIfPasswordResetCodeHasExpired(user);

            var viewModel = new ChooseNewPasswordViewModel {ResetCode = code};

            return View("ChooseNewPassword", viewModel);
        }
        
        [HttpGet("choose-new-password/complete")]
        public IActionResult ChooseNewPasswordCompleteGet()
        {
            // Redirect if already logged in
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
            }
            
            return View("ChooseNewPasswordComplete");
        }
        
        [ValidateAntiForgeryToken]
        [HttpPost("choose-new-password")]
        public IActionResult ChooseNewPasswordPost(ChooseNewPasswordViewModel viewModel)
        {
            // Redirect if already logged in
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
            }
            
            if (viewModel.NewPassword != viewModel.ConfirmNewPassword)
            {
                ModelState.AddModelError(nameof(viewModel.ConfirmNewPassword), "Password and confirmation password do not match");
            }

            if (!ModelState.IsValid)
            {
                return View("ChooseNewPassword", viewModel);
            }
            
            // Find the user from the reset code in the viewModel
            User userToUpdate = GetUserFromResetCode(viewModel.ResetCode);

            // Check that password reset code has not expired
            ThrowIfPasswordResetCodeHasExpired(userToUpdate);

            userRepository.UpdatePassword(userToUpdate, viewModel.NewPassword);
            emailSendingService.SendResetPasswordCompletedEmail(userToUpdate.EmailAddress);

            // Remove password reset code and send date
            RemovePasswordResetCode(userToUpdate);

            return RedirectToAction("ChooseNewPasswordCompleteGet");
        }
        
        // Look up the reset code (a GUID) in the database, and return the user it's associated with
        private User GetUserFromResetCode(string encryptedCode)
        {
            if (encryptedCode == null)
            {
                throw new PageNotFoundException();
            }
            
            User user = dataRepository.GetAll<User>().FirstOrDefault(u => u.PasswordResetCode == encryptedCode);
            
            // Check that user exists
            if (user == null)
            {
                throw new PageNotFoundException();
            }

            return user;
        }

        private static void ThrowIfPasswordResetCodeHasExpired(User user)
        {
            if (!user.ResetSendDate.HasValue)
            {
                throw new PasswordResetCodeExpiredException();
            }

            bool resetCodeHasExpired = (VirtualDateTime.Now - user.ResetSendDate.Value) > Global.PasswordResetCodeExpiryDays;
            if (resetCodeHasExpired)
            {
                throw new PasswordResetCodeExpiredException();
            }
        }

        private void RemovePasswordResetCode(User user)
        {
            user.PasswordResetCode = null;
            user.ResetSendDate = null;

            dataRepository.SaveChanges();
        }

        #endregion
    }
}
