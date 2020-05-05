using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Register;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebUI.Controllers
{
    public partial class RegisterController : BaseController
    {

        [Authorize]
        [HttpGet("service-activated")]
        public IActionResult ServiceActivated()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var model = this.UnstashModel<CompleteViewModel>(true);
            if (model == null)
            {
                return SessionExpiredView();
            }

            //Ensure the stash is cleared
            this.ClearStash();

            ReportingOrganisationId = model.OrganisationId;
            ViewBag.OrganisationName = ReportingOrganisation.OrganisationName;

            //Show the confirmation view
            return View("ServiceActivated");
        }

        [Authorize]
        [HttpGet("activate-service")]
        public async Task<IActionResult> ActivateService()
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

            if (userOrg == null)
            {
                throw new AuthenticationException();
            }

            //Ensure they havent entered wrong pin too many times
            TimeSpan remaining = userOrg.ConfirmAttemptDate == null
                ? TimeSpan.Zero
                : userOrg.ConfirmAttemptDate.Value.AddMinutes(Global.LockoutMinutes) - VirtualDateTime.Now;
            if (userOrg.ConfirmAttempts >= Global.MaxPinAttempts && remaining > TimeSpan.Zero)
            {
                return View("CustomError", new ErrorViewModel(1113, new {remainingTime = remaining.ToFriendly(maxParts: 2)}));
            }

            remaining = userOrg.PINSentDate == null
                ? TimeSpan.Zero
                : userOrg.PINSentDate.Value.AddDays(Global.PinInPostMinRepostDays) - VirtualDateTime.Now;
            var model = new CompleteViewModel();

            model.PIN = null;
            model.AllowResend = remaining <= TimeSpan.Zero;
            model.Remaining = remaining.ToFriendly(maxParts: 2);

            //If the email address is a test email then simulate sending
            if (userOrg.User.EmailAddress.StartsWithI(Global.TestPrefix))
            {
                model.PIN = "ABCDEF";
            }

            //Show the PIN textbox and button
            return View("ActivateService", model);
        }

        [PreventDuplicatePost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("activate-service")]
        public async Task<IActionResult> ActivateService(CompleteViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Ensure they have entered a PIN
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<CompleteViewModel>();
                return View("ActivateService", model);
            }

            //Get the user organisation
            UserOrganisation userOrg = await DataRepository.GetAll<UserOrganisation>()
                .FirstOrDefaultAsync(uo => uo.UserId == currentUser.UserId && uo.OrganisationId == ReportingOrganisationId);

            ActionResult result1;

            TimeSpan remaining = userOrg.ConfirmAttemptDate == null
                ? TimeSpan.Zero
                : userOrg.ConfirmAttemptDate.Value.AddMinutes(Global.LockoutMinutes) - VirtualDateTime.Now;
            if (userOrg.ConfirmAttempts >= Global.MaxPinAttempts && remaining > TimeSpan.Zero)
            {
                return View("CustomError", new ErrorViewModel(1113, new {remainingTime = remaining.ToFriendly(maxParts: 2)}));
            }

            var updateSearchIndex = false;
            if (PinMatchesPinInDatabase(userOrg, model.PIN))
            {
                //Set the user org as confirmed
                userOrg.PINConfirmedDate = VirtualDateTime.Now;

                //Set the pending organisation to active
                //Make sure the found organisation is active or pending

                if (userOrg.Organisation.Status.IsAny(OrganisationStatuses.Pending, OrganisationStatuses.Active))
                {
                    userOrg.Organisation.SetStatus(
                        OrganisationStatuses.Active,
                        OriginalUser == null ? currentUser.UserId : OriginalUser.UserId,
                        "PIN Confirmed");
                    updateSearchIndex = true;
                }
                else
                {
                    CustomLogger.Warning(
                        $"Attempt to PIN activate a {userOrg.Organisation.Status} organisation",
                        $"Organisation: '{userOrg.Organisation.OrganisationName}' Reference: '{userOrg.Organisation.EmployerReference}' User: '{currentUser.EmailAddress}'");
                    return View("CustomError", new ErrorViewModel(1149));
                }

                //Retire the old address 
                OrganisationAddress latestAddress = userOrg.Organisation.GetLatestAddress();
                if (latestAddress != null && latestAddress.AddressId != userOrg.Address.AddressId)
                {
                    latestAddress.SetStatus(
                        AddressStatuses.Retired,
                        OriginalUser == null ? currentUser.UserId : OriginalUser.UserId,
                        "Replaced by PIN in post");
                    updateSearchIndex = true;
                }

                //Activate the address the pin was sent to
                userOrg.Address.SetStatus(
                    AddressStatuses.Active,
                    OriginalUser == null ? currentUser.UserId : OriginalUser.UserId,
                    "PIN Confirmed");
                userOrg.ConfirmAttempts = 0;

                model.AccountingDate = userOrg.Organisation.SectorType.GetAccountingStartDate();
                model.OrganisationId = userOrg.OrganisationId;
                this.StashModel(model);

                result1 = RedirectToAction("ServiceActivated");

                //Send notification email to existing users 
                EmailSendingServiceHelpers.SendUserAddedEmailToExistingUsers(userOrg.Organisation, userOrg.User, emailSendingService);
            }
            else
            {
                userOrg.ConfirmAttempts++;
                AddModelError(3015, "PIN");
                result1 = View("ActivateService", model);
            }

            userOrg.ConfirmAttemptDate = VirtualDateTime.Now;

            //Save the changes
            await DataRepository.SaveChangesAsync();

            //Log the registration
            auditLogger.AuditChangeToUser(
                AuditedAction.RegistrationLog,
                userOrg.User,
                new
                {
                    Status = "PIN Confirmed",
                    Sector = userOrg.Organisation.SectorType,
                    Organisation = userOrg.Organisation.OrganisationName,
                    CompanyNo = userOrg.Organisation.CompanyNumber,
                    Address = userOrg.Address.GetAddressString(),
                    SicCodes = userOrg.Organisation.GetSicCodeIdsString(),
                    UserFirstname = userOrg.User.Firstname,
                    UserLastname = userOrg.User.Lastname,
                    UserJobtitle = userOrg.User.JobTitle,
                    UserEmail = userOrg.User.EmailAddress,
                    userOrg.User.ContactFirstName,
                    userOrg.User.ContactLastName,
                    userOrg.User.ContactJobTitle,
                    userOrg.User.ContactOrganisation,
                    userOrg.User.ContactPhoneNumber
                },
                currentUser);

            //Add this organisation to the search index
            if (updateSearchIndex)
            {
                await SearchBusinessLogic.UpdateSearchIndexAsync(userOrg.Organisation);
            }

            //Prompt the user with confirmation
            return result1;
        }

        private static bool PinMatchesPinInDatabase(UserOrganisation userOrg, string modelPin)
        {
            if (modelPin == null)
            {
                return false;
            }

            string normalisedPin = modelPin.Trim().ToUpper();
            if (!string.IsNullOrWhiteSpace(userOrg.PIN) && userOrg.PIN == normalisedPin)
            {
                return true;
            }

            return false;
        }

    }
}
