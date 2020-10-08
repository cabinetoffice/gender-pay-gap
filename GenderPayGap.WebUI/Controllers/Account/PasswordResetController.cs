using System;
using System.Linq;
using System.Web;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Models.Account;
using GenderPayGap.WebUI.Services;
using GovUkDesignSystem;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Account
{
    public class PasswordResetController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly IUserRepository userRepository;
        private readonly EmailSendingService emailSendingService;

        public PasswordResetController(
            IDataRepository dataRepository,
            IUserRepository userRepository,
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
            return View("PasswordReset", new PasswordResetViewModel());
        }

        [ValidateAntiForgeryToken]
        [PreventDuplicatePost]
        [HttpPost("password-reset")]
        public IActionResult PasswordResetPost(PasswordResetViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.EmailAddress);

            if (viewModel.HasAnyErrors())
            {
                return View("PasswordReset", viewModel); 
            }

            // Find user associated with email address
            User userForPasswordReset = userRepository.FindByEmailAsync(viewModel.EmailAddress, UserStatuses.Active).Result;

            if (userForPasswordReset.IsNull())
            {
                viewModel.AddErrorFor(m => m.EmailAddress, "An account associated with this email address does not exist.");
                
                return View("PasswordReset", viewModel); 
            }

            // If password reset email has been sent (and password hasn't been changed) within the last 10 minutes, show an error page
            if (PasswordResetEmailSentRecently(userForPasswordReset))
            {
                throw new UserRecentlySentPasswordResetEmailWithoutChangingPasswordException();
            }

            try
            {
                SendPasswordResetEmail(userForPasswordReset);
            }
            catch
            {
                throw new FailedToSendEmailException {EmailAddress = viewModel.EmailAddress};
            }

            return View("PasswordResetSent");
        }
        
        // Generates and stores a GUID to act as a password reset code
        private void SendPasswordResetEmail(User userForPasswordReset)
        {
            // Generate a random GUID as a unique identifier for the password reset
            string resetCode = Guid.NewGuid().ToString();

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
            // Don't use the User returned from this, but call it to check that the code is valid
            // And also has not expired
            ExtractUserFromResetCode(code);

            var viewModel = new ChooseNewPasswordViewModel {ResetCode = code};

            return View("ChooseNewPassword", viewModel);
        }
        
        [HttpGet("choose-new-password/complete")]
        public IActionResult ChooseNewPasswordCompleteGet()
        {
            return View("ChooseNewPasswordComplete");
        }
        
        [ValidateAntiForgeryToken]
        [PreventDuplicatePost]
        [HttpPost("choose-new-password")]
        public IActionResult ChooseNewPasswordPost(ChooseNewPasswordViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.NewPassword); 
            viewModel.ParseAndValidateParameters(Request, m => m.ConfirmNewPassword);
            
            if (viewModel.HasSuccessfullyParsedValueFor(m => m.NewPassword)
                && viewModel.HasSuccessfullyParsedValueFor(m => m.ConfirmNewPassword)
                && viewModel.NewPassword != viewModel.ConfirmNewPassword)
            {
                viewModel.AddErrorFor(m => m.ConfirmNewPassword, "Password and confirmation password do not match");
            }

            if (viewModel.HasAnyErrors())
            {
                return View("ChooseNewPassword", viewModel);
            }
            
            // Find the user from the reset code in the viewModel
            User userToUpdate = ExtractUserFromResetCode(viewModel.ResetCode);
            
            userRepository.UpdatePassword(userToUpdate, viewModel.NewPassword);
            emailSendingService.SendChangePasswordCompletedEmail(userToUpdate.EmailAddress);

            return RedirectToAction("ChooseNewPasswordCompleteGet");
        }
        
        // Look up the reset code (a GUID) in the database, and return the user it's associated with
        private User ExtractUserFromResetCode(string encryptedCode)
        {
            User user = dataRepository.GetAll<User>().FirstOrDefault(u => u.PasswordResetCode == encryptedCode);
            
            // Check that user exists
            if (user.IsNull())
            {
                throw new PageNotFoundException();
            }

            // Check that password reset code has not expired
            if (!PasswordResetCodeHasExpired(user))
            {
                throw new PasswordResetCodeExpiredException();
            }

            // Remove password reset code and send date
            user.PasswordResetCode = null;
            user.ResetSendDate = null;

            return user;
        }
        
        private static bool PasswordResetEmailSentRecently(User userForPasswordReset)
        {
            return userForPasswordReset.ResetSendDate.HasValue
                   && (DateTime.Now - userForPasswordReset.ResetSendDate.Value).TotalMinutes < Global.MinPasswordResetMinutes;
        }

        private static bool PasswordResetCodeHasExpired(User userForPasswordReset)
        {
            return userForPasswordReset.ResetSendDate.HasValue
                   && (DateTime.Now - userForPasswordReset.ResetSendDate.Value).TotalDays < Global.PasswordResetCodeExpiryDays;
        }


        #endregion
    }
}
