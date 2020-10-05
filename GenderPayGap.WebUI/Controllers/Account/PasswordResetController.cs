using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Account;
using GenderPayGap.WebUI.Repositories;
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

        #endregion

        #region Set new password

        [HttpGet("choose-new-password")]
        public IActionResult ChooseNewPasswordGet(string code)
        {
            string decryptedCode = DecryptAndDecodePasswordResetCode(code);

            User user = GetUserFromPasswordResetCode(decryptedCode);

            if (user.IsNull())
            {
                throw new ArgumentException("Invalid user id in password reset code");
            }

            ChooseNewPasswordViewModel viewModel = new ChooseNewPasswordViewModel {User = user};

            return View("ChooseNewPassword", viewModel);
        }
        
        [ValidateAntiForgeryToken]
        [PreventDuplicatePost]
        [HttpPost("choose-new-password")]
        public IActionResult ChooseNewPasswordPost(ChooseNewPasswordViewModel viewModel)
        {
            return null;
        }

        #endregion

        private static bool PasswordResetEmailSentRecently(User userForPasswordReset)
        {
            return userForPasswordReset.ResetSendDate.HasValue
                   && (userForPasswordReset.ResetSendDate.Value - DateTime.Now).TotalMinutes < Global.MinPasswordResetMinutes;
        }

        private void SendPasswordResetEmail(User userForPasswordReset)
        {
            // Send a password reset link to the user's email address
            string resetCode = Encryption.EncryptQuerystring(userForPasswordReset.UserId + ":" + VirtualDateTime.Now.ToSmallDateTime());
            // TODO: Update this action to an updated page
            string resetUrl = Url.Action("ChooseNewPasswordGet", "PasswordReset", new { code = resetCode }, "https");
            emailSendingService.SendResetPasswordVerificationEmail(userForPasswordReset.EmailAddress, resetUrl);

            CustomLogger.Information(
                "Password reset sent",
                $"User ID: {userForPasswordReset.UserId}, Email:{userForPasswordReset.EmailAddress}");
        }

        private string DecryptAndDecodePasswordResetCode(string encryptedCode)
        {
            string decryptedCode = Encryption.DecryptQuerystring(encryptedCode);
            
            return HttpUtility.UrlDecode(decryptedCode);
        }

        // Return the user from the user ID provided in the decrypted password reset code
        // Make sure to call DecryptAndDecodePasswordResetCode to decrypt the code before this
        private User GetUserFromPasswordResetCode(string code)
        {
            // Split the code by : to get individual segments
            string[] codeSegments = code.SplitI(":");
            
            // Convert the first segment (the user ID) to a long
            long userId = codeSegments[0].ToLong();

            // Find the user from this ID
            return dataRepository.Get<User>(userId);
        }

        private void CheckPasswordResetSendDate(string code)
        {
            // Split the code by : to get individual segments
            string[] codeSegments = code.SplitI(":");

            try
            {
                // Try to convert the segment to a DateTime
                DateTime passwordResetSendDate = DateTime.Parse(codeSegments[1]);

                // Check if the password reset email was sent more than X days ago
                // TODO: Take this number of days from config
                if ((DateTime.Now - passwordResetSendDate).TotalDays > 14)
                {
                    // TODO: Throw custom error
                }
            }
            catch
            {
                // Password reset date isn't valid
                throw new PageNotFoundException();
            }
            
        }

    }
}
