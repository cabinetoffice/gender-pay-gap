using System;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Register;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.Controllers
{
    public partial class RegisterController : BaseController
    {

        private async Task<DateTime> GetLastSignupDateAsync()
        {
            return await Cache.GetAsync<DateTime>($"{UserHostAddress}:LastSignupDate");
        }

        private async Task SetLastSignupDateAsync(DateTime value)
        {
            await Cache.RemoveAsync($"{UserHostAddress}:LastSignupDate");
            if (value > DateTime.MinValue)
            {
                await Cache.AddAsync($"{UserHostAddress}:LastSignupDate", value, value.AddMinutes(Global.MinSignupMinutes));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Redirect()
        {
            await WebTracker.TrackPageViewAsync(this);

            return RedirectToActionPermanent("AboutYou");
        }

        [HttpGet("about-you")]
        public async Task<IActionResult> AboutYou()
        {
            DateTime lastSignupDate = await GetLastSignupDateAsync();
            TimeSpan remainingTime = lastSignupDate == DateTime.MinValue
                ? TimeSpan.Zero
                : lastSignupDate.AddMinutes(Global.MinSignupMinutes) - VirtualDateTime.Now;
            if (!Global.SkipSpamProtection && remainingTime > TimeSpan.Zero)
            {
                return View("CustomError", new ErrorViewModel(1125, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
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

            var model = new RegisterViewModel();

            //Prepopulate with name and email saved during out-of-scope process
            string pendingFasttrackCodes = PendingFasttrackCodes;
            if (pendingFasttrackCodes != null)
            {
                string[] args = pendingFasttrackCodes?.SplitI(":");
                if (args.Length > 2)
                {
                    model.FirstName = args[2];
                }

                if (args.Length > 3)
                {
                    model.LastName = args[3];
                }

                if (args.Length > 4)
                {
                    model.EmailAddress = args[4];
                }

                model.ConfirmEmailAddress = model.EmailAddress;
            }

            //Start new user registration
            return View("AboutYou", model);
        }

        [HttpPost("about-you")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [SpamProtection]
        public async Task<IActionResult> AboutYou(RegisterViewModel model)
        {
            //Ensure IP address hasnt signed up recently
            DateTime lastSignupDate = await GetLastSignupDateAsync();
            TimeSpan remainingTime = lastSignupDate == DateTime.MinValue
                ? TimeSpan.Zero
                : lastSignupDate.AddMinutes(Global.MinSignupMinutes) - VirtualDateTime.Now;
            if (!Global.SkipSpamProtection && remainingTime > TimeSpan.Zero)
            {
                ModelState.AddModelError(3024, null, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)});
            }

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<RegisterViewModel>();
                return View("AboutYou", model);
            }

            //Validate the submitted fields
            if (model.Password.ContainsI("password"))
            {
                AddModelError(3000, "Password");
            }

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<RegisterViewModel>();
                return View("AboutYou", model);
            }

            //Ensure email is always lower case
            model.EmailAddress = model.EmailAddress.ToLower();

            //Check this email address isn't already assigned to another user
            User currentUser = await UserRepository.FindByEmailAsync(model.EmailAddress, UserStatuses.New, UserStatuses.Active);
            if (currentUser != null)
            {
                if (currentUser.EmailVerifySendDate != null)
                {
                    if (currentUser.EmailVerifiedDate != null)
                    {
                        //A registered user with this email already exists.
                        AddModelError(3001, "EmailAddress");
                        this.CleanModelErrors<RegisterViewModel>();
                        return View("AboutYou", model);
                    }

                    remainingTime = currentUser.EmailVerifySendDate.Value.AddHours(Global.EmailVerificationExpiryHours)
                                    - VirtualDateTime.Now;
                    if (remainingTime > TimeSpan.Zero)
                    {
                        AddModelError(3002, "EmailAddress", new {remainingTime = remainingTime.ToFriendly(maxParts: 2)});
                        this.CleanModelErrors<RegisterViewModel>();
                        return View("AboutYou", model);
                    }
                }

                //Delete the previous user org if there is one
                UserOrganisation userOrg = await DataRepository.GetAll<UserOrganisation>()
                    .FirstOrDefaultAsync(uo => uo.UserId == currentUser.UserId);
                if (userOrg != null)
                {
                    DataRepository.Delete(userOrg);
                }

                //If from a previous user then delete the previous user
                DataRepository.Delete(currentUser);
            }

            //Save the submitted fields
            currentUser = new User();
            currentUser.Created = VirtualDateTime.Now;
            currentUser.Modified = currentUser.Created;
            currentUser.Firstname = model.FirstName;
            currentUser.Lastname = model.LastName;
            currentUser.JobTitle = model.JobTitle;
            if (model.EmailAddress.StartsWithI(Global.TestPrefix))
            {
                currentUser.EmailAddressDB = model.EmailAddress;
            }
            else
            {
                currentUser.EmailAddress = model.EmailAddress;
            }

            if (!currentUser.IsAdministrator())
            {
                currentUser.SetSetting(UserSettingKeys.AllowContact, model.AllowContact.ToString());
                currentUser.SetSetting(UserSettingKeys.SendUpdates, model.SendUpdates.ToString());
            }

            UserRepository.UpdateUserPasswordUsingPBKDF2(currentUser, model.Password);

            currentUser.EmailVerifySendDate = null;
            currentUser.EmailVerifiedDate = null;
            currentUser.EmailVerifyHash = null;

            //Save the user with new status
            currentUser.SetStatus(UserStatuses.New, OriginalUser ?? currentUser);

            // save the current user
            DataRepository.Insert(currentUser);
            await DataRepository.SaveChangesAsync();

            //Save pendingFasttrackCodes
            string pendingFasttrackCodes = PendingFasttrackCodes;
            if (pendingFasttrackCodes != null)
            {
                string[] args = pendingFasttrackCodes?.SplitI(":");
                pendingFasttrackCodes = $"{args[0]}:{args[1]}";
                currentUser.SetSetting(UserSettingKeys.PendingFasttrackCodes, pendingFasttrackCodes);
                await DataRepository.SaveChangesAsync();
                PendingFasttrackCodes = null;
            }

            //Send the verification code and show confirmation
            this.StashModel(model);

            //Ensure signup is restricted to every 10 min
            await SetLastSignupDateAsync(model.EmailAddress.StartsWithI(Global.TestPrefix) ? DateTime.MinValue : VirtualDateTime.Now);

            return RedirectToAction("VerifyEmail");
        }

    }
}
