using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Autofac.Features.AttributeFilters;
using GenderPayGap.BusinessLogic;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.Models.Register;
using GenderPayGap.WebUI.Models.Scope;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("Register")]
    public partial class RegisterController : BaseController
    {

        private readonly PinInThePostService pinInThePostService;
        private readonly EmailSendingService emailSendingService;
        private readonly AuditLogger auditLogger;

        #region Constructors

        public RegisterController(
            IHttpCache cache,
            IHttpSession session,
            IScopePresentation scopePresentation,
            IScopeBusinessLogic scopeBL,
            IOrganisationBusinessLogic orgBL,
            IUserRepository userRepository,
            IDataRepository dataRepository,
            IWebTracker webTracker,
            PinInThePostService pinInThePostService,
            EmailSendingService emailSendingService,
            [KeyFilter("Private")] IPagedRepository<EmployerRecord> privateSectorRepository,
            [KeyFilter("Public")] IPagedRepository<EmployerRecord> publicSectorRepository,
            AuditLogger auditLogger)
            : base(
            cache,
            session,
            dataRepository,
            webTracker)
        {
            ScopePresentation = scopePresentation;
            ScopeBusinessLogic = scopeBL;
            OrganisationBusinessLogic = orgBL;
            PrivateSectorRepository = privateSectorRepository;
            PublicSectorRepository = publicSectorRepository;
            UserRepository = userRepository;
            this.pinInThePostService = pinInThePostService;
            this.emailSendingService = emailSendingService;
            this.auditLogger = auditLogger;
        }

        #endregion

        #region Dependencies

        // TODO move these in to RegisterPresentation service
        public IScopePresentation ScopePresentation { get; }
        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; }
        public IScopeBusinessLogic ScopeBusinessLogic { get; }
        public IUserRepository UserRepository { get; }
        public IPagedRepository<EmployerRecord> PrivateSectorRepository { get; }
        public IPagedRepository<EmployerRecord> PublicSectorRepository { get; }

        #endregion

        #region Session & Cache Properties

        protected async Task<DateTime> GetLastPasswordResetDateAsync()
        {
            return await Cache.GetAsync<DateTime>($"{UserHostAddress}:LastPasswordResetDate");
        }

        protected async Task SetLastPasswordResetDateAsync(DateTime value)
        {
            await Cache.RemoveAsync($"{UserHostAddress}:LastPasswordResetDate");
            if (value > DateTime.MinValue)
            {
                await Cache.AddAsync($"{UserHostAddress}:LastPasswordResetDate", value, value.AddMinutes(Global.MinPasswordResetMinutes));
            }
        }


        private int LastPrivateSearchRemoteTotal => Session["LastPrivateSearchRemoteTotal"].ToInt32();

        private int CompaniesHouseFailures
        {
            get => Session["CompaniesHouseFailures"].ToInt32();
            set
            {
                Session.Remove("CompaniesHouseFailures");
                if (value > 0)
                {
                    Session["CompaniesHouseFailures"] = value;
                }
            }
        }

        #endregion
        
        #region PINSent

        private async Task<IActionResult> GetSendPINAsync()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Get the user organisation
            UserOrganisation userOrg = await DataRepository.GetAll<UserOrganisation>()
                .FirstOrDefaultAsync(uo => uo.UserId == currentUser.UserId && uo.OrganisationId == ReportingOrganisationId);

            //If a pin has never been sent or resend button submitted then send one immediately
            if (string.IsNullOrWhiteSpace(userOrg.PIN)
                || userOrg.PINSentDate.EqualsI(null, DateTime.MinValue)
                || userOrg.PINSentDate.Value.AddDays(Global.PinInPostExpiryDays) < VirtualDateTime.Now)
            {
                try
                {
                    DateTime now = VirtualDateTime.Now;

                    // Check if we are a test user (for load testing)
                    bool thisIsATestUser = userOrg.User.EmailAddress.StartsWithI(Global.TestPrefix);

                    // Generate a new pin
                    string pin = OrganisationBusinessLogic.GeneratePINCode(thisIsATestUser);

                    // Save the PIN and confirm code
                    userOrg.PIN = pin;
                    userOrg.PINSentDate = now;
                    userOrg.Method = RegistrationMethods.PinInPost;
                    await DataRepository.SaveChangesAsync();

                    if (!thisIsATestUser)
                    {
                        // Try and send the PIN in post
                        if (pinInThePostService.SendPinInThePost(this, userOrg, pin, out string letterId))
                        {
                            userOrg.PITPNotifyLetterId = letterId;
                            await DataRepository.SaveChangesAsync();
                        }
                        else
                        {
                            // Show "Notify is down" error message
                            return View(
                                "PinFailedToSend",
                                new PinFailedToSendViewModel {OrganisationName = userOrg.Organisation.OrganisationName});
                        }
                    }

                    CustomLogger.Information(
                        "Send Pin-in-post",
                        $"Name {currentUser.Fullname}, Email:{currentUser.EmailAddress}, IP:{UserHostAddress}, Address:{userOrg?.Address.GetAddressString()}");
                }
                catch (Exception ex)
                {
                    CustomLogger.Error(ex.Message, ex);
                    // TODO: maybe change this?
                    return View("CustomError", new ErrorViewModel(3014));
                }
            }

            //Prepare view parameters
            ViewBag.UserFullName = currentUser.Fullname;
            ViewBag.UserJobTitle = currentUser.JobTitle;
            ViewBag.Organisation = userOrg.Organisation.OrganisationName;
            ViewBag.Address = userOrg?.Address.GetAddressString(",<br/>");
            return View("PINSent");
        }

        [Authorize]
        [HttpGet("pin-sent")]
        public async Task<IActionResult> PINSent()
        {
            //Clear the stash
            this.ClearStash();

            return await GetSendPINAsync();
        }

        #endregion

        #region RequestPIN

        [HttpGet("request-pin")]
        [Authorize]
        public async Task<IActionResult> RequestPIN()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Get the user organisation
            UserOrganisation userOrg = await DataRepository.GetAll<UserOrganisation>()
                .FirstOrDefaultAsync(uo => uo.UserId == currentUser.UserId && uo.OrganisationId == ReportingOrganisationId);

            //Prepare view parameters
            ViewBag.UserFullName = currentUser.Fullname;
            ViewBag.UserJobTitle = currentUser.JobTitle;
            ViewBag.Organisation = userOrg.Organisation.OrganisationName;
            ViewBag.Address = userOrg?.Address.GetAddressString(",<br/>");
            //Show the PIN textbox and button
            return View("RequestPIN");
        }

        [PreventDuplicatePost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("request-pin")]
        public async Task<IActionResult> RequestPIN(CompleteViewModel model)
        {
            
            if (FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.PrivateManualRegistration) )
            {
                return RedirectToAction("ManageOrganisations", "Organisation");
            }
            
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Get the user organisation
            UserOrganisation userOrg = await DataRepository.GetAll<UserOrganisation>()
                .FirstOrDefaultAsync(uo => uo.UserId == currentUser.UserId && uo.OrganisationId == ReportingOrganisationId);

            //Mark the user org as ready to send a pin
            userOrg.PIN = null;
            userOrg.PINSentDate = null;
            await DataRepository.SaveChangesAsync();

            return RedirectToAction("PINSent");
        }

        #endregion

        #region password-reset

        [HttpGet("password-reset")]
        public async Task<IActionResult> PasswordReset()
        {
            //Ensure IP address hasnt signed up recently
            DateTime lastPasswordResetDate = await GetLastPasswordResetDateAsync();
            TimeSpan remainingTime = lastPasswordResetDate == DateTime.MinValue
                ? TimeSpan.Zero
                : lastPasswordResetDate.AddMinutes(Global.MinPasswordResetMinutes) - VirtualDateTime.Now;
            if (!Global.SkipSpamProtection && remainingTime > TimeSpan.Zero)
            {
                return View("CustomError", new ErrorViewModel(1133, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
            }

            User currentUser;
            //Ensure user has not completed the registration process
            IActionResult result = CheckUserRegisteredOk(out currentUser);
            if (result != null)
            {
                return result;
            }

            //Clear the stash
            this.ClearStash();

            //Start new user registration
            return View("PasswordReset", new ResetViewModel());
        }

        [SpamProtection(5)]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("password-reset")]
        public async Task<IActionResult> PasswordReset(ResetViewModel model)
        {
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.ConfirmPassword));

            //Ensure IP address hasnt signed up recently
            DateTime lastPasswordResetDate = await GetLastPasswordResetDateAsync();
            TimeSpan remainingTime = lastPasswordResetDate == DateTime.MinValue
                ? TimeSpan.Zero
                : lastPasswordResetDate.AddMinutes(Global.MinPasswordResetMinutes) - VirtualDateTime.Now;
            if (!Global.SkipSpamProtection && remainingTime > TimeSpan.Zero)
            {
                ModelState.AddModelError(3026, null, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)});
            }

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<RegisterViewModel>();
                return View("PasswordReset", model);
            }

            //Ensure user has not completed the registration process
            IActionResult result = CheckUserRegisteredOk(out User currentUser);
            if (result != null)
            {
                return result;
            }

            //Validate the submitted fields
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ResetViewModel>();
                return View("PasswordReset", model);
            }

            //Ensure email is always lower case
            model.EmailAddress = model.EmailAddress.ToLower();
            ViewBag.EmailAddress = model.EmailAddress;

            //Ensure signup is restricted to every 10 mins
            await SetLastPasswordResetDateAsync(
                model.EmailAddress.StartsWithI(Global.TestPrefix) ? DateTime.MinValue : VirtualDateTime.Now);

            // find the latest active user by email
            currentUser = await UserRepository.FindByEmailAsync(model.EmailAddress, UserStatuses.Active);
            if (currentUser == null)
            {
                CustomLogger.Warning(
                    "Password reset requested for unknown email address",
                    $"Email:{model.EmailAddress}, IP:{UserHostAddress}");
                return View("PasswordResetSent");
            }

            if (!ResendPasswordReset(currentUser))
            {
                AddModelError(1122);
                this.CleanModelErrors<ResetViewModel>();
                return View("PasswordReset", model);
            }

            currentUser.ResetAttempts = 0;
            currentUser.ResetSendDate = VirtualDateTime.Now;
            await DataRepository.SaveChangesAsync();

            //show confirmation
            ViewBag.EmailAddress = currentUser.EmailAddress;
            if (currentUser.EmailAddress.StartsWithI(Global.TestPrefix))
            {
                ViewBag.TestUrl = Url.Action(
                    "NewPassword",
                    "Register",
                    new {code = Encryption.EncryptQuerystring(currentUser.UserId + ":" + VirtualDateTime.Now.ToSmallDateTime())},
                    "https");
            }

            return View("PasswordResetSent");
        }

        private bool ResendPasswordReset(User currentUser)
        {
            //Send a password reset link to the email address
            string resetCode = null;
            try
            {
                resetCode = Encryption.EncryptQuerystring(currentUser.UserId + ":" + VirtualDateTime.Now.ToSmallDateTime());
                string resetUrl = Url.Action("NewPassword", "Register", new { code = resetCode }, "https");
                emailSendingService.SendResetPasswordVerificationEmail(currentUser.EmailAddress, resetUrl);

                CustomLogger.Information(
                    "Password reset sent",
                    $"Name {currentUser.Fullname}, Email:{currentUser.EmailAddress}, IP:{UserHostAddress}");
            }
            catch (Exception ex)
            {
                //Log the exception
                CustomLogger.Error(ex.Message, ex);
                return false;
            }

            return true;
        }

        [HttpGet("enter-new-password")]
        public IActionResult NewPassword(string code = null)
        {
            User currentUser;
            //Ensure user has not completed the registration process
            IActionResult result = CheckUserRegisteredOk(out currentUser);
            if (result != null)
            {
                return result;
            }

            result = UnwrapPasswordReset(code, out currentUser);
            if (result != null)
            {
                return result;
            }

            var model = new ResetViewModel();
            model.Resetcode = code;
            this.StashModel(model);

            //Start new user registration
            return View("NewPassword", model);
        }

        private ActionResult UnwrapPasswordReset(string code, out User user)
        {
            user = null;

            long userId = 0;
            DateTime resetDate;
            try
            {
                code = Encryption.DecryptQuerystring(code);
                code = HttpUtility.UrlDecode(code);
                string[] args = code.SplitI(":");
                if (args.Length != 2)
                {
                    throw new ArgumentException("Too few parameters in password reset code");
                }

                userId = args[0].ToLong();
                if (userId == 0)
                {
                    throw new ArgumentException("Invalid user id in password reset code");
                }

                resetDate = args[1].FromSmallDateTime();
                if (resetDate == DateTime.MinValue)
                {
                    throw new ArgumentException("Invalid password reset date in password reset code");
                }
            }
            catch
            {
                return View("CustomError", new ErrorViewModel(1123));
            }

            //Get the user oganisation
            user = DataRepository.Get<User>(userId);

            if (user == null)
            {
                return View("CustomError", new ErrorViewModel(1124));
            }

            if (resetDate.AddDays(1) < VirtualDateTime.Now)
            {
                return View("CustomError", new ErrorViewModel(1126));
            }

            return null;
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("enter-new-password")]
        public async Task<IActionResult> NewPassword(ResetViewModel model)
        {
            User currentUser;
            //Ensure user has not completed the registration process
            IActionResult result = CheckUserRegisteredOk(out currentUser);
            if (result != null)
            {
                return result;
            }

            ModelState.Remove(nameof(model.EmailAddress));
            ModelState.Remove(nameof(model.ConfirmEmailAddress));

            //Validate the submitted fields
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ResetViewModel>();
                return View("NewPassword", model);
            }

            var m = this.UnstashModel<ResetViewModel>();
            if (m == null || string.IsNullOrWhiteSpace(m.Resetcode))
            {
                return View("CustomError", new ErrorViewModel(0));
            }

            result = UnwrapPasswordReset(m.Resetcode, out currentUser);
            if (result != null)
            {
                return result;
            }

            this.ClearStash();

            //Save the user to ensure UserId>0 for new status
            UserRepository.UpdateUserPasswordUsingPBKDF2(currentUser, model.Password);

            currentUser.ResetAttempts = 0;
            currentUser.ResetSendDate = null;
            await DataRepository.SaveChangesAsync();

            //Send completed notification email
            emailSendingService.SendResetPasswordCompletedEmail(currentUser.EmailAddress);

            //Send the verification code and showconfirmation
            return View("CustomError", new ErrorViewModel(1127));
        }

        #endregion

    }
}
