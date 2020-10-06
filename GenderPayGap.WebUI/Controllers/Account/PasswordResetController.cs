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
            User userForPasswordReset = DecryptResetCodeAndValidateComponents(code);

            ChooseNewPasswordViewModel viewModel = new ChooseNewPasswordViewModel {User = userForPasswordReset};

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

        // Creates the reset code by encrypting the user ID for the user to change password
        // A colon separator so the information can be extracted
        // And the DateTime of sending, in ISO1806 format
        private void SendPasswordResetEmail(User userForPasswordReset)
        {
            // Send a password reset link to the user's email address
            string resetCode = Encryption.EncryptQuerystring(userForPasswordReset.UserId + ":" + VirtualDateTime.Now.ToString("yyyyMMddHHmmss"));
            // TODO: Update this action to an updated page
            string resetUrl = Url.Action("ChooseNewPasswordGet", "PasswordReset", new { code = resetCode }, "https");
            emailSendingService.SendResetPasswordVerificationEmail(userForPasswordReset.EmailAddress, resetUrl);

            CustomLogger.Information(
                "Password reset sent",
                $"User ID: {userForPasswordReset.UserId}, Email:{userForPasswordReset.EmailAddress}");
        }

        // Decrypt and decode the encrypted code from the password reset link
        // Extract the user ID, look for the user and check that it exists
        // Extract the reset code send date and check that it has not expired
        private User DecryptResetCodeAndValidateComponents(string encryptedCode)
        {
            string decryptedCode = DecryptAndDecodePasswordResetCode(encryptedCode);

            User user = GetUserFromPasswordResetCode(decryptedCode);

            if (user.IsNull())
            {
                throw new PageNotFoundException();
            }
            
            ValidatePasswordResetSendDate(decryptedCode);

            return user;
        }

        private string DecryptAndDecodePasswordResetCode(string encryptedCode)
        {
            string decryptedCode = Encryption.DecryptQuerystring(encryptedCode);
            
            return HttpUtility.UrlDecode(decryptedCode.ToString());
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

        // Check that the password reset code contains a valid date
        // And that is has not expired
        private void ValidatePasswordResetSendDate(string code)
        {
            // Split the code by : to get individual segments
            string[] codeSegments = code.SplitI(":");

            try
            {
                // Try to convert the segment to a DateTime
                DateTime passwordResetSendDate = DateTime.Parse(codeSegments[1]);

                // Check if the password reset email was sent more than PasswordResetCodeExpiryDays ago (from Global.cs)
                if ((DateTime.Now - passwordResetSendDate).TotalDays > Global.PasswordResetCodeExpiryDays)
                {
                    throw new PasswordResetCodeExpiredException();
                }
            }
            catch
            {
                // Password reset date isn't valid (DateTime.Parse fails on the string)
                throw new PageNotFoundException();
            }
            
        }

    }
}
