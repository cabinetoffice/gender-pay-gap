using System;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Models.AccountCreation;
using GenderPayGap.WebUI.Services;
using GovUkDesignSystem;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Account

{
    public class AccountCreationController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly EmailSendingService emailSendingService;

        public AccountCreationController(
            IDataRepository dataRepository,
            EmailSendingService emailSendingService)
        {
            this.dataRepository = dataRepository;
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
                    return RedirectToAction("CreateUserAccountGet", new { isPartOfGovUkReportingJourney = true});

                case HaveYouAlreadyCreatedYourUserAccount.Unspecified:
                    viewModel.AddErrorFor(
                        m => m.HaveYouAlreadyCreatedYourUserAccount,
                        "You must select whether you have already created your user account");
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

            return View("CreateUserAccount", new CreateUserAccountViewModel{IsPartOfGovUkReportingJourney = isPartOfGovUkReportingJourney });
        }

        [HttpPost("/create-user-account")]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUserAccountPost(CreateUserAccountViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.EmailAddress);
            viewModel.ParseAndValidateParameters(Request, m => m.ConfirmEmailAddress);
            viewModel.ParseAndValidateParameters(Request, m => m.FirstName);
            viewModel.ParseAndValidateParameters(Request, m => m.LastName);
            viewModel.ParseAndValidateParameters(Request, m => m.JobTitle);
            viewModel.ParseAndValidateParameters(Request, m => m.Password);
            viewModel.ParseAndValidateParameters(Request, m => m.ConfirmPassword);

            if (viewModel.HasSuccessfullyParsedValueFor(m => m.EmailAddress)
                && viewModel.HasSuccessfullyParsedValueFor(m => m.ConfirmEmailAddress)
                && viewModel.EmailAddress != viewModel.ConfirmEmailAddress)
            {
                viewModel.AddErrorFor(
                    m => m.ConfirmEmailAddress,
                    "The email address and confirmation do not match.");
            }

            if (viewModel.HasSuccessfullyParsedValueFor(m => m.Password)
                && viewModel.HasSuccessfullyParsedValueFor(m => m.ConfirmPassword)
                && viewModel.Password != viewModel.ConfirmPassword)
            {
                viewModel.AddErrorFor(
                    m => m.ConfirmPassword,
                    "The password and confirmation do not match.");
            }

            User existingUser = CheckForExistingUserForGivenEmailAddress(viewModel.EmailAddress);
            if (existingUser?.EmailVerifySendDate != null)
            {
                if (existingUser.EmailVerifiedDate != null)
                {
                    viewModel.AddErrorFor(
                        m => m.EmailAddress,
                        "This email address has already been registered. Please sign in or enter a different email "
                        + "address.");
                }
                else
                {
                    viewModel.AddErrorFor(
                        m => m.EmailAddress,
                        "This email address is awaiting confirmation. Please check you email inbox or enter a different email"
                        + " address");
                }
            }

            if (viewModel.HasAnyErrors())
            {
                return View("CreateUserAccount", viewModel);
            }

            User newUser = CreateNewUser(viewModel);
            dataRepository.Insert(newUser);
            dataRepository.SaveChanges();

            GenerateAndSendAccountVerificationEmail(newUser);

            var confirmEmailAddressViewModel = new ConfirmEmailAddressViewModel {EmailAddress = viewModel.EmailAddress};
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

        private User CheckForExistingUserForGivenEmailAddress(string emailAddress)
        {
            return dataRepository
                .GetAll<User>()
                .Where(u => u.Status == UserStatuses.New || u.Status == UserStatuses.Active)
                .AsEnumerable( /* Needed to prevent "The LINQ expression could not be translated" - string.Equals cannot be translated */ )
                .FirstOrDefault(u => string.Equals(u.EmailAddress, emailAddress, StringComparison.CurrentCultureIgnoreCase));
        }

        private User CreateNewUser(CreateUserAccountViewModel viewModel)
        {
            var user = new User();
            user.Created = VirtualDateTime.Now;
            user.Modified = user.Created;
            user.Firstname = viewModel.FirstName;
            user.Lastname = viewModel.LastName;
            user.JobTitle = viewModel.JobTitle;
            user.EmailAddress = viewModel.EmailAddress;
            user.AllowContact = viewModel.AllowContact;
            user.SendUpdates = viewModel.SendUpdates;

            byte[] salt = Crypto.GetSalt();
            user.Salt = Convert.ToBase64String(salt);
            user.PasswordHash = Crypto.GetPBKDF2(viewModel.Password, salt);
            user.HashingAlgorithm = HashingAlgorithm.PBKDF2;

            user.EmailVerifySendDate = null;
            user.EmailVerifiedDate = null;
            user.EmailVerifyHash = null;
            user.SetStatus(UserStatuses.New, user);
            user.Status = UserStatuses.New;

            return user;
        }

        private void GenerateAndSendAccountVerificationEmail(User user)
        {
            string verificationCode = Guid.NewGuid().ToString("N");
            string verificationUrl = Url.Action(
                "VerifyEmail",
                "AccountCreation",
                new {code = verificationCode},
                "https");

            try
            {
                emailSendingService.SendAccountVerificationEmail(user.EmailAddress, verificationUrl);
                user.EmailVerifyHash = verificationCode;
                user.EmailVerifySendDate = VirtualDateTime.Now;

                dataRepository.SaveChanges();
            }
            catch
            {
                // help user resend email
                throw new Exception("Failed to send verification email. Please try again");
            }
        }

    }
}
