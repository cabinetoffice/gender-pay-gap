﻿using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.AccountCreation;
using GenderPayGap.WebUI.Repositories;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Account

{
    public class AccountCreationController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly UserRepository userRepository;
        private readonly EmailSendingService emailSendingService;

        public AccountCreationController(
            IDataRepository dataRepository,
            UserRepository userRepository,
            EmailSendingService emailSendingService)
        {
            this.dataRepository = dataRepository;
            this.userRepository = userRepository;
            this.emailSendingService = emailSendingService;
        }

        // The 'Start Now' page (Global.StartUrl https://www.gov.uk/report-gender-pay-gap-data)
        // links to this action as the starting point for the reporting journey
        [HttpGet("/already-created-an-account-question")]
        public IActionResult AlreadyCreatedAnAccountQuestionGet(AlreadyCreatedAnAccountViewModel viewModel)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
            }

            switch (viewModel.HaveYouAlreadyCreatedYourUserAccount)
            {
                case HaveYouAlreadyCreatedYourUserAccount.Yes:
                    return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");

                case HaveYouAlreadyCreatedYourUserAccount.No:
                case HaveYouAlreadyCreatedYourUserAccount.NotSure:
                    return RedirectToAction("CreateUserAccountGet", new { isPartOfGovUkReportingJourney = true });

                case HaveYouAlreadyCreatedYourUserAccount.Unspecified:
                    ModelState.AddModelError(nameof(viewModel.HaveYouAlreadyCreatedYourUserAccount), "You must select whether you have already created your user account");
                    return View("AlreadyCreatedAnAccountQuestion", viewModel);

                default:
                    // This serves as the initial GET case
                    var model = new AlreadyCreatedAnAccountViewModel();
                    return View("AlreadyCreatedAnAccountQuestion", model);
            }
        }

        [HttpGet("/create-user-account")]
        public IActionResult CreateUserAccountGet(bool isPartOfGovUkReportingJourney = false)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
            }

            return View("CreateUserAccount", new CreateUserAccountViewModel { IsPartOfGovUkReportingJourney = isPartOfGovUkReportingJourney });
        }

        [HttpPost("/create-user-account")]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUserAccountPost(CreateUserAccountViewModel viewModel)
        {
            if (viewModel.EmailAddress != null && 
                viewModel.ConfirmEmailAddress != null && 
                viewModel.EmailAddress != viewModel.ConfirmEmailAddress)
            {
                ModelState.AddModelError(nameof(viewModel.ConfirmEmailAddress), "The email address and confirmation do not match.");
            }

            if (viewModel.Password != null && 
                viewModel.ConfirmPassword != null && 
                viewModel.Password != viewModel.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(viewModel.ConfirmPassword), "The password and confirmation do not match.");
            }
            
            if (!ModelState.IsValid)
            {
                return View("CreateUserAccount", viewModel);
            }

            User existingUser = userRepository.FindByEmail(viewModel.EmailAddress, UserStatuses.Active, UserStatuses.New);
            if (existingUser?.EmailVerifySendDate != null)
            {
                if (existingUser.EmailVerifiedDate != null)
                {
                    ModelState.AddModelError(nameof(viewModel.EmailAddress), "This email address has already been registered. "
                                                                             + "Please sign in or enter a different email address.");
                }
                else
                {
                    ModelState.AddModelError(nameof(viewModel.EmailAddress), "This email address is awaiting confirmation. "
                                                                             + "Please check your email inbox or enter a different email address");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("CreateUserAccount", viewModel);
            }

            // Check for retired account with matching email address
            User retiredUser = userRepository.FindByEmail(viewModel.EmailAddress, UserStatuses.Retired);

            var user = CreateNewOrUpdateRetiredUser(viewModel, retiredUser);
                
            if (retiredUser == null)
            {
                dataRepository.Insert(user);
            }

            GenerateAndSendAccountVerificationEmail(user);

            dataRepository.SaveChanges();

            var confirmEmailAddressViewModel = new ConfirmEmailAddressViewModel { EmailAddress = viewModel.EmailAddress };
            return View("ConfirmEmailAddress", confirmEmailAddressViewModel);
        }

        [HttpGet("/verify-email")]
        public IActionResult VerifyEmail(string code)
        {
            User gpgUser = dataRepository.GetAll<User>().FirstOrDefault(u => u.EmailVerifyHash == code);

            if (gpgUser == null)
            {
                return View("UserNotFoundErrorPage");
            }

            if (User.Identity.IsAuthenticated || gpgUser.EmailVerifiedDate != null)
            {
                return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
            }

            gpgUser.EmailVerifiedDate = VirtualDateTime.Now;
            gpgUser.SetStatus(UserStatuses.Active, gpgUser, "Email verified");
            dataRepository.SaveChanges();

            return RedirectToAction("AccountCreationConfirmation");
        }

        [HttpGet("account-creation-confirmation")]
        public IActionResult AccountCreationConfirmation()
        {
            return View("ConfirmationPage");
        }

        private User CreateNewOrUpdateRetiredUser(CreateUserAccountViewModel viewModel, User retiredUser = null)
        {
            var currentTime = VirtualDateTime.Now;

            // If user creates a new account with same email address as retired account, reuse the old
            // account to avoid duplicates, add status change details and update all info except created date.
            var user = retiredUser ?? new User();
            var createdDate = retiredUser == null ? currentTime : retiredUser.Created;
            var statusDetails = retiredUser == null ? null : "Retired user account has been reactivated";

            user.Created = createdDate;
            user.Modified = currentTime;
            user.Firstname = viewModel.FirstName;
            user.Lastname = viewModel.LastName;
            user.JobTitle = viewModel.JobTitle;
            user.EmailAddress = viewModel.EmailAddress;
            user.AllowContact = viewModel.AllowContact;
            user.SendUpdates = viewModel.SendUpdates;

            byte[] salt = PasswordHelper.GetSalt();
            user.Salt = Convert.ToBase64String(salt);
            user.PasswordHash = PasswordHelper.GetPBKDF2(viewModel.Password, salt);
            user.HashingAlgorithm = HashingAlgorithm.PBKDF2;

            user.EmailVerifySendDate = null;
            user.EmailVerifiedDate = null;
            user.EmailVerifyHash = null;
            user.SetStatus(UserStatuses.New, user, statusDetails);
            user.Status = UserStatuses.New;

            return user;
        }

        private void GenerateAndSendAccountVerificationEmail(User user)
        {
            string verificationCode = Guid.NewGuid().ToString("N");
            string verificationUrl = Url.Action(
                "VerifyEmail",
                "AccountCreation",
                new { code = verificationCode },
                "https");

            try
            {
                emailSendingService.SendAccountVerificationEmail(user.EmailAddress, verificationUrl);
                user.EmailVerifyHash = verificationCode;
                user.EmailVerifySendDate = VirtualDateTime.Now;
            }
            catch
            {
                // help user resend email
                throw new Exception("Failed to send verification email. Please try again");
            }
        }

    }
}
