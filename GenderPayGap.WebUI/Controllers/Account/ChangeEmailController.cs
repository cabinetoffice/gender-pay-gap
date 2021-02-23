using System;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Account;
using GenderPayGap.WebUI.Services;
using GovUkDesignSystem;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Account
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("manage-account")]
    public class ChangeEmailController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly IUserRepository userRepository;
        private readonly EmailSendingService emailSendingService;

        public ChangeEmailController(
            IDataRepository dataRepository,
            IUserRepository userRepository,
            EmailSendingService emailSendingService)
        {
            this.dataRepository = dataRepository;
            this.userRepository = userRepository;
            this.emailSendingService = emailSendingService;
        }


        // The user asks to change their email address
        // We ask them for their new email address
        [HttpGet("change-email")]
        public IActionResult ChangeEmailGet()
        {
            var viewModel = new ChangeEmailViewModel();
            return View("ChangeEmail", viewModel);
        }

        // The user tells us their new email address
        // We validate it (e.g. is it the email address of another user)
        // Then, we send a verification email
        [ValidateAntiForgeryToken]
        [HttpPost("change-email")]
        public IActionResult ChangeEmailPost(ChangeEmailViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.NewEmailAddress);

            if (viewModel.HasAnyErrors())
            {
                return View("ChangeEmail", viewModel);
            }

            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            if (viewModel.NewEmailAddress == user.EmailAddress)
            {
                viewModel.AddErrorFor(m => m.NewEmailAddress, "The new email address must be different to your current email address");
                return View("ChangeEmail", viewModel);
            }

            if (OtherUserWithThisEmailAddressAlreadyExists(viewModel.NewEmailAddress))
            {
                viewModel.AddErrorFor(m => m.NewEmailAddress, "This email address is already taken by another user");
                return View("ChangeEmail", viewModel);
            }

            SendVerificationEmail(viewModel.NewEmailAddress, user);

            return View("PleaseVerifyNewEmailAddress", viewModel.NewEmailAddress);
        }

        private void SendVerificationEmail(string newEmailAddress, User userToVerify)
        {
            string code = CreateEmailVerificationCode(newEmailAddress, userToVerify);

            string returnVerifyUrl = GenerateChangeEmailVerificationUrl(code);

            emailSendingService.SendChangeEmailPendingVerificationEmail(newEmailAddress, returnVerifyUrl);
        }

        private string CreateEmailVerificationCode(string newEmailAddress, User user)
        {
            return Encryption.EncryptModel(
                new ChangeEmailVerificationToken
                {
                    UserId = user.UserId,
                    NewEmailAddress = newEmailAddress.ToLower(),
                    TokenTimestamp = VirtualDateTime.Now
                });
        }

        private string GenerateChangeEmailVerificationUrl(string code)
        {
            return Url.Action("VerifyEmailGet", "ChangeEmail", new { code }, "https");
        }


        // The user has clicked on the verification email
        // We validate the verification token (e.g. has it expired)
        // We ask for their password, to check they own the account
        // (e.g. in case they typed in someone else's email address by mistake
        //  and the other person tried to take control of their account!)
        [AllowAnonymous]
        [HttpGet("verify-email")]
        public IActionResult VerifyEmailGet(string code)
        {
            ChangeEmailVerificationToken changeEmailToken = Encryption.DecryptModel<ChangeEmailVerificationToken>(code);

            if (TokenHasExpired(changeEmailToken))
            {
                string error = "Your email verification link has expired. Please go to My Account and start the email change process again.";
                return View("VerifyEmailError", error);
            }

            User user = dataRepository.Get<User>(changeEmailToken.UserId);

            var viewModel = new VerifyEmailChangeViewModel
            {
                User = user,
                Code = code,
                NewEmailAddress = changeEmailToken.NewEmailAddress
            };
            return View("VerifyEmail", viewModel);
        }

        // The user has typed in their password
        // We validate the email verify token, and check that their password is correct
        // We then go ahead and change their email address
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost("verify-email")]
        public IActionResult VerifyEmailPost(VerifyEmailChangeViewModel viewModel)
        {
            ChangeEmailVerificationToken changeEmailToken = Encryption.DecryptModel<ChangeEmailVerificationToken>(viewModel.Code);

            if (TokenHasExpired(changeEmailToken))
            {
                string error = "Your email verification link has expired. Please go to My Account and start the email change process again.";
                return View("VerifyEmailError", error);
            }

            User user = dataRepository.Get<User>(changeEmailToken.UserId);
            viewModel.User = user;
            viewModel.NewEmailAddress = changeEmailToken.NewEmailAddress;

            // Check if the user has entered a password (they might have left this field blank)
            viewModel.ParseAndValidateParameters(Request, m => m.Password);
            if (viewModel.HasAnyErrors())
            {
                return View("VerifyEmail", viewModel);
            }

            if (!userRepository.CheckPassword(user, viewModel.Password))
            {
                viewModel.AddErrorFor(m => m.Password, "Incorrect password");
                return View("VerifyEmail", viewModel);
            }

            if (OtherUserWithThisEmailAddressAlreadyExists(viewModel.NewEmailAddress))
            {
                string error = "This email address is already taken by another account.";
                return View("VerifyEmailError", error);
            }

            string oldEmailAddress = user.EmailAddress;

            userRepository.UpdateEmail(user, changeEmailToken.NewEmailAddress);

            NotifyBothOldAndNewEmailAddressesThatEmailAddressHasBeenChanged(oldEmailAddress, changeEmailToken.NewEmailAddress);

            return View("ChangeEmailComplete", changeEmailToken.NewEmailAddress);
        }

        private static bool TokenHasExpired(ChangeEmailVerificationToken changeEmailToken)
        {
            DateTime verifyExpiryDate = changeEmailToken.TokenTimestamp.AddDays(Global.EmailVerificationExpiryDays);
            return verifyExpiryDate < VirtualDateTime.Now;
        }

        private void NotifyBothOldAndNewEmailAddressesThatEmailAddressHasBeenChanged(string oldEmailAddress, string newEmailAddress)
        {
            // send to old email
            emailSendingService.SendChangeEmailCompletedNotificationEmail(oldEmailAddress);

            // send to new email
            emailSendingService.SendChangeEmailCompletedVerificationEmail(newEmailAddress);
        }


        private bool OtherUserWithThisEmailAddressAlreadyExists(string emailAddress)
        {
            User otherUserWithSameEmailAddress = userRepository.FindByEmail(emailAddress, UserStatuses.New, UserStatuses.Active);
            return otherUserWithSameEmailAddress != null;
        }

    }
}
