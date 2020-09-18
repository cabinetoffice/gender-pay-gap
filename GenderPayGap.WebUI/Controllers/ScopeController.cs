using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.Models.Scope;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{

    [Route("scope")]
    public class ScopeController : BaseController
    {
        private readonly EmailSendingService emailSendingService;

        #region Constructors

        public ScopeController(
            IHttpCache cache,
            IHttpSession session,
            IScopePresentation scopeUI,
            IDataRepository dataRepository,
            IWebTracker webTracker,
            EmailSendingService emailSendingService)
            : base(cache, session, dataRepository, webTracker)
        {
            ScopePresentation = scopeUI;
            this.emailSendingService = emailSendingService;
        }

        #endregion

        #region Dependencies

        public IScopePresentation ScopePresentation { get; }

        #endregion

        [HttpGet("out")]
        public async Task<IActionResult> OutOfScope()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("AdminHomePage", "AdminHomepage");
            }

            var currentStateModel = this.UnstashModel<ScopingViewModel>();
            EnterCodesViewModel model = currentStateModel?.EnterCodes ?? new EnterCodesViewModel();

            // when spamlocked then return a CustomError view
            TimeSpan remainingTime = await GetRetryLockRemainingTimeAsync("lastScopeCode", Global.LockoutMinutes);
            if (remainingTime > TimeSpan.Zero)
            {
                return View("CustomError", new ErrorViewModel(1125, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
            }

            PendingFasttrackCodes = null;

            // show the view
            return View("EnterCodes", model);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("out")]
        public async Task<IActionResult> OutOfScope(EnterCodesViewModel model)
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("AdminHomePage", "AdminHomepage");
            }

            // When Spamlocked then return a CustomError view
            TimeSpan remainingTime = await GetRetryLockRemainingTimeAsync("lastScopeCode", Global.LockoutMinutes);
            if (remainingTime > TimeSpan.Zero)
            {
                return View("CustomError", new ErrorViewModel(1125, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
            }

            // the following fields are validatable at this stage
            ModelState.Include(
                nameof(EnterCodesViewModel.EmployerReference),
                nameof(EnterCodesViewModel.SecurityToken));

            // When ModelState is Not Valid Then Return the EnterCodes View
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<EnterCodesViewModel>();
                return View("EnterCodes", model);
            }

            // Generate the state model
            ScopingViewModel stateModel = await ScopePresentation.CreateScopingViewModelAsync(model, CurrentUser);

            if (stateModel == null)
            {
                await IncrementRetryCountAsync("lastScopeCode", Global.LockoutMinutes);
                ModelState.AddModelError(3027);
                this.CleanModelErrors<EnterCodesViewModel>();
                return View("EnterCodes", model);
            }


            //Clear the retry locks
            await ClearRetryLocksAsync("lastScopeCode");

            // set the back link
            stateModel.StartUrl = Url.Action("OutOfScope");

            // set the journey to out-of-scope
            stateModel.IsOutOfScopeJourney = true;

            // save the state to the session cache
            this.StashModel(stateModel);

            // when security code has expired then redirect to the CodeExpired action
            if (stateModel.IsSecurityCodeExpired)
            {
                return View("CodeExpired", stateModel);
            }


            //When on out-of-scope journey and any previous explicit scope then tell user scope is known
            if (!stateModel.IsChangeJourney
                && (
                    stateModel.LastScope != null && stateModel.LastScope.ScopeStatus.IsAny(ScopeStatuses.InScope, ScopeStatuses.OutOfScope)
                    || stateModel.ThisScope != null
                    && stateModel.ThisScope.ScopeStatus.IsAny(ScopeStatuses.InScope, ScopeStatuses.OutOfScope)
                )
            )
            {
                return View("ScopeKnown", stateModel);
            }

            // redirect to next step
            return RedirectToAction("ConfirmOutOfScopeDetails");
        }

        private async Task IncrementRetryCountAsync(string retryLockKey, int expiryMinutes)
        {
            int count = await Cache.GetAsync<int>($"{UserHostAddress}:{retryLockKey}:Count");
            count++;
            if (count >= 3)
            {
                await CreateRetryLockAsync(retryLockKey, expiryMinutes);
            }

            await Cache.RemoveAsync($"{UserHostAddress}:{retryLockKey}:Count");
            await Cache.AddAsync($"{UserHostAddress}:{retryLockKey}:Count", count, VirtualDateTime.Now.AddMinutes(expiryMinutes));
        }

        private async Task CreateRetryLockAsync(string retryLockKey, int expiryMinutes)
        {
            await Cache.RemoveAsync($"{UserHostAddress}:{retryLockKey}");
            await Cache.AddAsync($"{UserHostAddress}:{retryLockKey}", VirtualDateTime.Now, VirtualDateTime.Now.AddMinutes(expiryMinutes));
        }

        private async Task<TimeSpan> GetRetryLockRemainingTimeAsync(string retryLockKey, int expiryMinutes)
        {
            if (Global.SkipSpamProtection)
            {
                return TimeSpan.Zero;
            }

            DateTime lockDate = await Cache.GetAsync<DateTime>($"{UserHostAddress}:{retryLockKey}");
            TimeSpan remainingTime =
                lockDate == DateTime.MinValue ? TimeSpan.Zero : lockDate.AddMinutes(expiryMinutes) - VirtualDateTime.Now;
            return remainingTime;
        }

        private async Task ClearRetryLocksAsync(string retryLockKey)
        {
            await Cache.RemoveAsync($"{UserHostAddress}:{retryLockKey}");
            await Cache.RemoveAsync($"{UserHostAddress}:{retryLockKey}:Count");
        }


        [Authorize]
        [HttpGet("in")]
        public async Task<IActionResult> InScope()
        {
            await WebTracker.TrackPageViewAsync(this);

            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("AdminHomePage", "AdminHomepage");
            }

            return RedirectToAction(nameof(OrganisationController.ManageOrganisations), "Organisation");
        }

        [Authorize]
        [HttpGet("in/confirm")]
        public IActionResult ConfirmInScope()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("AdminHomePage", "AdminHomepage");
            }

            var stateModel = this.UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            // else redirect to ConfirmDetails action
            return View("ConfirmInScope", stateModel);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("in/confirm")]
        public async Task<IActionResult> ConfirmInScope(string command)
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("AdminHomePage", "AdminHomepage");
            }

            var stateModel = this.UnstashModel<ScopingViewModel>(true);
            // when model is null then return session expired view
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            ApplyUserContactDetails(CurrentUser, stateModel);

            // Save user as in scope
            var snapshotYears = new HashSet<int> {stateModel.AccountingDate.Year};
            await ScopePresentation.SaveScopesAsync(stateModel, snapshotYears);

            var organisation = DataRepository.Get<Organisation>(stateModel.OrganisationId);
            DateTime currentSnapshotDate = organisation.SectorType.GetAccountingStartDate();
            if (stateModel.AccountingDate == currentSnapshotDate)
            {
                IEnumerable<string> emailAddressesForOrganisation = organisation.UserOrganisations.Select(uo => uo.User.EmailAddress);
                foreach (string emailAddress in emailAddressesForOrganisation)
                {
                    emailSendingService.SendScopeChangeInEmail(emailAddress, organisation.OrganisationName);
                }
            }

            //Start new user registration
            return RedirectToAction(
                "ManageOrganisation",
                "Organisation",
                new {id = Encryption.EncryptQuerystring(stateModel.OrganisationId.ToString())});
        }


        [HttpGet("out/confirm-employer")]
        public IActionResult ConfirmOutOfScopeDetails()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("AdminHomePage", "AdminHomepage");
            }

            var stateModel = this.UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            // else redirect to ConfirmDetails action
            return View("ConfirmOutOfScopeDetails", stateModel);
        }

        [HttpGet("out/questions")]
        public IActionResult EnterOutOfScopeAnswers()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("AdminHomePage", "AdminHomepage");
            }

            var stateModel = this.UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            return View("EnterOutOfScopeAnswers", stateModel);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("out/questions")]
        public IActionResult EnterOutOfScopeAnswers(EnterAnswersViewModel enterAnswersModel)
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("AdminHomePage", "AdminHomepage");
            }

            // update the state model
            var stateModel = this.UnstashModel<ScopingViewModel>();
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            // update the state
            stateModel.EnterAnswers = enterAnswersModel;
            this.StashModel(stateModel);
            var fields = new List<string>();

            // when the user is not logged in then validate the contact details
            if (CurrentUser == null)
            {
                fields.Add(nameof(EnterAnswersViewModel.FirstName));
                fields.Add(nameof(EnterAnswersViewModel.LastName));
                fields.Add(nameof(EnterAnswersViewModel.EmailAddress));
                fields.Add(nameof(EnterAnswersViewModel.ConfirmEmailAddress));
            }

            // the following fields are validatable at this stage
            fields.Add(nameof(EnterAnswersViewModel.Reason));
            if (enterAnswersModel.Reason == "Other")
            {
                fields.Add(nameof(EnterAnswersViewModel.OtherReason));
            }

            fields.Add(nameof(EnterAnswersViewModel.ReadGuidance));

            ModelState.Include(fields.ToArray());

            // validate the details
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ScopingViewModel>();
                return View("EnterOutOfScopeAnswers", stateModel);
            }

            //Ensure email is always lower case
            if (!string.IsNullOrEmpty(enterAnswersModel.EmailAddress))
            {
                enterAnswersModel.EmailAddress = enterAnswersModel.EmailAddress.ToLower();
            }

            this.StashModel(stateModel);

            //Start new user registration
            return RedirectToAction("ConfirmOutOfScopeAnswers");
        }

        [HttpGet("out/confirm-answers")]
        public IActionResult ConfirmOutOfScopeAnswers()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("AdminHomePage", "AdminHomepage");
            }

            var stateModel = this.UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            return View("ConfirmOutOfScopeAnswers", stateModel);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("out/confirm-answers")]
        public async Task<IActionResult> ConfirmOutOfScopeAnswers(string command)
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("AdminHomePage", "AdminHomepage");
            }

            var stateModel = this.UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            ApplyUserContactDetails(CurrentUser, stateModel);

            // Save user as out of scope
            var snapshotYears = new HashSet<int> {stateModel.AccountingDate.Year};
            if (!stateModel.IsChangeJourney)
            {
                snapshotYears.Add(stateModel.AccountingDate.Year - 1);
            }

            await ScopePresentation.SaveScopesAsync(stateModel, snapshotYears);

            this.StashModel(stateModel);

            var organisation = DataRepository.Get<Organisation>(stateModel.OrganisationId);
            DateTime currentSnapshotDate = organisation.SectorType.GetAccountingStartDate();
            if (stateModel.AccountingDate == currentSnapshotDate)
            {
                IEnumerable<string> emailAddressesForOrganisation = organisation.UserOrganisations.Select(uo => uo.User.EmailAddress);
                foreach (string emailAddress in emailAddressesForOrganisation)
                {
                    emailSendingService.SendScopeChangeOutEmail(emailAddress, organisation.OrganisationName);
                }
            }

            //Start new user registration
            return RedirectToAction("FinishOutOfScope", "Scope");
        }

        [HttpGet("out/finish")]
        public IActionResult FinishOutOfScope()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("AdminHomePage", "AdminHomepage");
            }

            var stateModel = this.UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            this.StashModel(stateModel);

            //Complete
            return View("FinishOutOfScope", stateModel);
        }

        [HttpGet("register")]
        public IActionResult RegisterOrManage()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && CurrentUser.IsAdministrator())
            {
                return RedirectToAction("AdminHomePage", "AdminHomepage");
            }

            var stateModel = this.UnstashModel<ScopingViewModel>(true);
            // when model is null then return session expired view
            if (stateModel == null)
            {
                return SessionExpiredView();
            }

            //if user has already registered then manage that organisation
            if (stateModel.UserIsRegistered)
            {
                return RedirectToAction(
                    "ManageOrganisation",
                    "Organisation",
                    new {id = Encryption.EncryptQuerystring(stateModel.OrganisationId.ToString())});
            }

            // when not auth then save codes and return ManageOrganisations redirect
            if (!stateModel.IsSecurityCodeExpired)
            {
                PendingFasttrackCodes =
                    $"{stateModel.EnterCodes.EmployerReference}:{stateModel.EnterCodes.SecurityToken}:{stateModel.EnterAnswers?.FirstName}:{stateModel.EnterAnswers?.LastName}:{stateModel.EnterAnswers?.EmailAddress}";
            }

            return RedirectToAction(nameof(OrganisationController.ManageOrganisations), "Organisation");
        }

        private void ApplyUserContactDetails(User currentUser, ScopingViewModel model)
        {
            // when logged in then override contact details
            if (currentUser != null)
            {
                model.EnterAnswers.FirstName = currentUser.Firstname;
                model.EnterAnswers.LastName = currentUser.Lastname;
                model.EnterAnswers.EmailAddress = currentUser.EmailAddress;
            }
        }

    }

}
