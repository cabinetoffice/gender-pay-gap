using System;
using System.Linq;
using System.Security.Claims;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.AccountCreation;
using GenderPayGap.WebUI.Services;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Account

{
    public class AccountCreationController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AccountCreationController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("/prototype/start-page")]
        public IActionResult StartPage()
        {
            return View("StartPagePrototype");
        }

        [HttpGet("/prototype/already-created-an-account-question")]
        public IActionResult AlreadyCreatedAnAccountQuestionGet(AlreadyCreatedAnAccountViewModel viewModel)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ManageOrganisations", "Organisation");
            }

            switch (viewModel.HaveYouAlreadyCreatedYourUserAccount)
            {
                case null:
                    var model = new AlreadyCreatedAnAccountViewModel();
                    return View("AlreadyCreatedAnAccountQuestion", model);

                case HaveYouAlreadyCreatedYourUserAccount.Unspecified:
                    viewModel.AddErrorFor<AlreadyCreatedAnAccountViewModel, HaveYouAlreadyCreatedYourUserAccount?>(
                        m => m.HaveYouAlreadyCreatedYourUserAccount,
                        "You must select an option before continuing");
                    return View("AlreadyCreatedAnAccountQuestion", viewModel);

                case HaveYouAlreadyCreatedYourUserAccount.Yes:
                    return RedirectToAction("ManageOrganisations", "Organisation");

                default:
                    return RedirectToAction("CreateUserAccountGet");
            }
        }

        [HttpGet("/prototype/create-user-account")]
        public IActionResult CreateUserAccountGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ManageOrganisations", "Organisation");
            }

            return View("CreateUserAccount", new CreateUserAccountViewModel());
        }

        [HttpPost("/prototype/create-user-account")]
        public IActionResult CreateUserAccountPost(CreateUserAccountViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.EmailAddress);
            viewModel.ParseAndValidateParameters(Request, m => m.ConfirmEmailAddress);
            viewModel.ParseAndValidateParameters(Request, m => m.FirstName);
            viewModel.ParseAndValidateParameters(Request, m => m.LastName);
            viewModel.ParseAndValidateParameters(Request, m => m.JobTitle);
            viewModel.ParseAndValidateParameters(Request, m => m.Password);
            viewModel.ParseAndValidateParameters(Request, m => m.ConfirmPassword);

            if (viewModel.HasAnyErrors())
            {
                return View("CreateUserAccount", viewModel);
            }

            if (viewModel.EmailAddress != viewModel.ConfirmEmailAddress)
            {
                viewModel.AddErrorFor<CreateUserAccountViewModel, string>(
                    m => m.ConfirmEmailAddress,
                    "The email address and confirmation do not match.");

                return View("CreateUserAccount", viewModel);
            }

            if (viewModel.Password != viewModel.ConfirmPassword)
            {
                viewModel.AddErrorFor<CreateUserAccountViewModel, string>(
                    m => m.ConfirmPassword,
                    "The password and confirmation do not match.");

                return View("CreateUserAccount", viewModel);
            }

            User existingUser = CheckForExistingUserForGivenEmailAddress(viewModel.EmailAddress);

            if (existingUser?.EmailVerifySendDate != null)
            {
                if (existingUser.EmailVerifiedDate != null)
                {
                    viewModel.AddErrorFor<CreateUserAccountViewModel, string>(
                        m => m.EmailAddress,
                        "This email address has already been registered. Please sign in or enter a different email "
                        + "address.");
                    return View("CreateUserAccount", viewModel);
                }
                else
                {
                    viewModel.AddErrorFor<CreateUserAccountViewModel, string>(
                        m => m.EmailAddress,
                        "This email address is awaiting confirmation. Please check you email inbox or enter a different email"
                        + " address");
                    return View("CreateUserAccount", viewModel);
                }
            }

            User newUser = CreateNewUser(viewModel);
            dataRepository.Insert(newUser);
            dataRepository.SaveChangesAsync().Wait();

            string verificationCode = GenerateVerificationCode(newUser);
            string verificationUrl = Url.Action(
                "VerifyEmail",
                "AccountCreation",
                new {code = verificationCode},
                "https");

            try
            {
                EmailSendingService.PrototypeSendAccountVerificationEmail(viewModel.EmailAddress, verificationUrl);
                newUser.EmailVerifyHash = Crypto.GetSHA512Checksum(verificationCode);
                newUser.EmailVerifySendDate = VirtualDateTime.Now;

                dataRepository.SaveChangesAsync().Wait();
            }
            catch
            {
                // help user resend email
                throw new Exception("Failed to send verification email. Please try again");
            }

            var confirmEmailAddressViewModel = new ConfirmEmailAddressViewModel {EmailAddress = viewModel.EmailAddress};
            return View("ConfirmEmailAddress", confirmEmailAddressViewModel);
        }

        [HttpGet("/prototype/verify-email")]
        public IActionResult VerifyEmail(string code)
        {
            User gpgUser = GetGpgUserFromAspNetUser(User, dataRepository);

            if (User.Identity.IsAuthenticated || gpgUser.EmailVerifiedDate != null)
            {
                return RedirectToAction("ManageOrganisations", "Organisation");
            }

            if (gpgUser.EmailVerifySendDate == null)
            {
                // email not sent
                // help user resend email
                return RedirectToAction("Index", "Viewing");
            }

            // TODO when moving from prototype to production code this config value should supersede Global.EmailVerificationExpiryHours
            // and be shared with the Functions.Purge webjob
            if (gpgUser.EmailVerifySendDate.Value.AddDays(7) < VirtualDateTime.Now)
            {
                // code expired
                // help user resend email
                return RedirectToAction("Index", "Viewing");
            }

            if (gpgUser.EmailVerifyHash != Crypto.GetSHA512Checksum(code))
            {
                // wrong code
                // help user resend email
                return RedirectToAction("Index", "Viewing");
            }

            gpgUser.EmailVerifiedDate = VirtualDateTime.Now;
            gpgUser.SetStatus(UserStatuses.Active, gpgUser, "Email verified");

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
                .Where(u => string.Equals(u.EmailAddress, emailAddress, StringComparison.CurrentCultureIgnoreCase))
                .FirstOrDefault(u => u.Status == UserStatuses.New || u.Status == UserStatuses.Active);
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
            user.SetSetting(UserSettingKeys.AllowContact, viewModel.AllowContact.ToString());
            user.SetSetting(UserSettingKeys.SendUpdates, viewModel.SendUpdates.ToString());

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

        private string GenerateVerificationCode(User user)
        {
            return Encryption.EncryptQuerystring(user.UserId + ":" + user.Created.ToSmallDateTime());
        }

        private static User GetGpgUserFromAspNetUser(ClaimsPrincipal user, IDataRepository dataRepository)
        {
            if (user != null && user.Identity.IsAuthenticated)
            {
                return dataRepository.FindUser(user);
            }

            return null;
        }

    }
}
