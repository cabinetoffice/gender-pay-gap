using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GenderPayGap.Core;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
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

        private ActionResult UnwrapRegistrationRequest(OrganisationViewModel model, out UserOrganisation userOrg)
        {
            userOrg = null;

            long userId = 0;
            long orgId = 0;
            try
            {
                string code = Encryption.DecryptQuerystring(model.ReviewCode);
                code = HttpUtility.UrlDecode(code);
                string[] args = code.SplitI(":");
                if (args.Length != 3)
                {
                    throw new ArgumentException("Too few parameters in registration review code");
                }

                userId = args[0].ToLong();
                if (userId == 0)
                {
                    throw new ArgumentException("Invalid user id in registration review code");
                }

                orgId = args[1].ToLong();
                if (orgId == 0)
                {
                    throw new ArgumentException("Invalid organisation id in registration review code");
                }
            }
            catch
            {
                return View("CustomError", new ErrorViewModel(1114));
            }

            //Get the user oganisation
            userOrg = DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.UserId == userId && uo.OrganisationId == orgId);

            if (userOrg == null)
            {
                return View("CustomError", new ErrorViewModel(1115));
            }

            //Check this registrations hasnt already completed
            if (userOrg.PINConfirmedDate != null)
            {
                return View("CustomError", new ErrorViewModel(1145));
            }

            switch (userOrg.Organisation.Status)
            {
                case OrganisationStatuses.Active:
                case OrganisationStatuses.Pending:
                    break;
                default:
                    throw new ArgumentException(
                        $"Invalid organisation status {userOrg.Organisation.Status} user {userId} and organisation {orgId} for reviewing registration request");
            }

            if (userOrg.Address == null)
            {
                throw new Exception($"Cannot find address for user {userId} and organisation {orgId} for reviewing registration request");
            }

            //Load view model
            model.ContactFirstName = userOrg.User.ContactFirstName;
            model.ContactLastName = userOrg.User.ContactLastName;
            if (string.IsNullOrWhiteSpace(model.ContactFirstName) && string.IsNullOrWhiteSpace(model.ContactFirstName))
            {
                model.ContactFirstName = userOrg.User.Firstname;
                model.ContactLastName = userOrg.User.Lastname;
            }

            model.ContactJobTitle = userOrg.User.ContactJobTitle.Coalesce(userOrg.User.JobTitle);
            model.ContactEmailAddress = userOrg.User.ContactEmailAddress.Coalesce(userOrg.User.EmailAddress);
            model.EmailAddress = userOrg.User.EmailAddress;
            model.ContactPhoneNumber = userOrg.User.ContactPhoneNumber;

            model.OrganisationName = userOrg.Organisation.OrganisationName;
            model.CompanyNumber = userOrg.Organisation.CompanyNumber;
            model.SectorType = userOrg.Organisation.SectorType;
            model.SicCodes = userOrg.Organisation.GetSicCodes().Select(o => o.SicCode.SicCodeId).ToList();

            model.Address1 = userOrg.Address.Address1;
            model.Address2 = userOrg.Address.Address2;
            model.Address3 = userOrg.Address.Address3;
            model.Country = userOrg.Address.Country;
            model.Postcode = userOrg.Address.PostCode;
            model.PoBox = userOrg.Address.PoBox;

            model.RegisteredAddress = userOrg.Address.Status == AddressStatuses.Pending
                ? userOrg.Organisation.GetLatestAddress()?.GetAddressString()
                : null;

            model.CharityNumber = userOrg.Organisation.OrganisationReferences
                .Where(o => o.ReferenceName.ToLower() == nameof(OrganisationViewModel.CharityNumber).ToLower())
                .Select(or => or.ReferenceValue)
                .FirstOrDefault();

            model.MutualNumber = userOrg.Organisation.OrganisationReferences
                .Where(o => o.ReferenceName.ToLower() == nameof(OrganisationViewModel.MutualNumber).ToLower())
                .Select(or => or.ReferenceValue)
                .FirstOrDefault();

            model.OtherName = userOrg.Organisation.OrganisationReferences.ToList()
                .Where(
                    o => o.ReferenceName.ToLower() != nameof(OrganisationViewModel.CharityNumber).ToLower()
                         && o.ReferenceName.ToLower() != nameof(OrganisationViewModel.MutualNumber).ToLower())
                .Select(or => or.ReferenceName)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(model.OtherName))
            {
                model.OtherValue = userOrg.Organisation.OrganisationReferences
                    .Where(o => o.ReferenceName == model.OtherName)
                    .Select(or => or.ReferenceValue)
                    .FirstOrDefault();
            }

            return null;
        }

        #region ReviewRequest

        [Authorize(Roles = "GPGadmin")]
        [HttpGet("review-request")]
        public async Task<IActionResult> ReviewRequest(string code)
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var model = new OrganisationViewModel();

            if (string.IsNullOrWhiteSpace(code))
            {
                //Load the employers from session
                model = this.UnstashModel<OrganisationViewModel>();
                if (model == null)
                {
                    return View("CustomError", new ErrorViewModel(1114));
                }
            }
            else
            {
                model.ReviewCode = code;
            }

            //Unwrap code
            UserOrganisation userOrg;
            ActionResult result = UnwrapRegistrationRequest(model, out userOrg);
            if (result != null)
            {
                return result;
            }

            //Tell reviewer if this org has already been approved
            if (model.ManualRegistration)
            {
                UserOrganisation firstRegistered = userOrg.Organisation.UserOrganisations.OrderByDescending(uo => uo.PINConfirmedDate)
                    .FirstOrDefault(uo => uo.PINConfirmedDate != null);
                if (firstRegistered != null)
                {
                    AddModelError(
                        3017,
                        parameters: new
                        {
                            approvedUser = firstRegistered.User.EmailAddress,
                            approvedDate = firstRegistered.PINConfirmedDate.Value.ToShortDateString(),
                            approvedAddress = firstRegistered.Address?.GetAddressString()
                        });
                }

                //Tell reviewer how many other open regitrations for same organisation
                int requestCount = await DataRepository.GetAll<UserOrganisation>()
                    .CountAsync(
                        uo => uo.UserId != userOrg.UserId
                              && uo.OrganisationId == userOrg.OrganisationId
                              && uo.Organisation.Status == OrganisationStatuses.Pending);
                if (requestCount > 0)
                {
                    AddModelError(3018, parameters: new {requestCount});
                }
            }

            //Get any conflicting or similar organisations
            IEnumerable<long> results;
            var orgIds = new HashSet<long>();

            if (!string.IsNullOrWhiteSpace(model.CompanyNumber))
            {
                results = DataRepository.GetAll<Organisation>()
                    .Where(
                        o => o.OrganisationId != userOrg.OrganisationId
                             && o.SectorType == SectorTypes.Private
                             && o.CompanyNumber == model.CompanyNumber)
                    .Select(o => o.OrganisationId);
                if (results.Any())
                {
                    orgIds.AddRange(results);
                }
            }

            if (!string.IsNullOrWhiteSpace(model.CharityNumber))
            {
                results = DataRepository.GetAll<OrganisationReference>()
                    .Where(
                        r => r.OrganisationId != userOrg.OrganisationId
                             && r.ReferenceName.ToLower() == "charity number"
                             && r.ReferenceValue.ToLower() == model.CharityNumber.ToLower())
                    .Select(r => r.OrganisationId);
                if (results.Any())
                {
                    orgIds.AddRange(results);
                }
            }

            if (!string.IsNullOrWhiteSpace(model.MutualNumber))
            {
                results = DataRepository.GetAll<OrganisationReference>()
                    .Where(
                        r => r.OrganisationId != userOrg.OrganisationId
                             && r.ReferenceName.ToLower() == "mutual number"
                             && r.ReferenceValue.ToLower() == model.MutualNumber.ToLower())
                    .Select(r => r.OrganisationId);
                if (results.Any())
                {
                    orgIds.AddRange(results);
                }
            }

            if (!string.IsNullOrWhiteSpace(model.OtherName) && !string.IsNullOrWhiteSpace(model.OtherValue))
            {
                results = DataRepository.GetAll<OrganisationReference>()
                    .Where(
                        r => r.OrganisationId != userOrg.OrganisationId
                             && r.ReferenceName.ToLower() == model.OtherName.ToLower()
                             && r.ReferenceValue.ToLower() == model.OtherValue.ToLower())
                    .Select(r => r.OrganisationId);
                if (results.Any())
                {
                    orgIds.AddRange(results);
                }
            }

            model.MatchedReferenceCount = orgIds.Count;

            //Only show orgs matching names when none matching references
            if (model.MatchedReferenceCount == 0)
            {
                string orgName = model.OrganisationName.ToLower().ReplaceI("limited", "").ReplaceI("ltd", "");
                results = DataRepository.GetAll<Organisation>()
                    .Where(o => o.OrganisationId != userOrg.OrganisationId && o.OrganisationName.ToLower().Contains(orgName))
                    .Select(o => o.OrganisationId);
                if (results.Any())
                {
                    orgIds.AddRange(results);
                }

                results = Organisation.Search(
                        DataRepository.GetAll<Organisation>().Where(o => o.OrganisationId != userOrg.OrganisationId),
                        model.OrganisationName,
                        50 - results.Count(),
                        Global.LevenshteinDistance)
                    .Select(o => o.OrganisationId);
                if (results.Any())
                {
                    orgIds.AddRange(results);
                }
            }

            if (orgIds.Any())
            {
                //Add the registrations
                List<Organisation> orgs =
                    await DataRepository.GetAll<Organisation>().Where(o => orgIds.Contains(o.OrganisationId)).ToListAsync();
                model.ManualEmployers = orgs.Select(o => o.ToEmployerRecord()).ToList();
            }

            //Ensure exact match shown at top
            if (model.ManualEmployers != null)
            {
                if (model.ManualEmployers.Count > 1)
                {
                    int index = model.ManualEmployers.FindIndex(e => e.OrganisationName.EqualsI(model.OrganisationName));
                    if (index > 0)
                    {
                        model.ManualEmployers.Insert(0, model.ManualEmployers[index]);
                        model.ManualEmployers.RemoveAt(index + 1);
                    }
                }

                //Sort he organisations
                model.ManualEmployers = model.ManualEmployers.OrderBy(o => o.OrganisationName).ToList();
            }

            this.StashModel(model);
            return View("ReviewRequest", model);
        }

        [Authorize(Roles = "GPGadmin")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("review-request")]
        public async Task<IActionResult> ReviewRequest(OrganisationViewModel model, string command)
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Make sure we can load employers from session
            var m = this.UnstashModel<OrganisationViewModel>();
            if (m == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            model.ManualEmployers = m.ManualEmployers;

            //Unwrap code
            UserOrganisation userOrg;
            ActionResult result = UnwrapRegistrationRequest(model, out userOrg);
            if (result != null)
            {
                return result;
            }

            //Check model is valid

            //Exclude the address details
            var excludes = new HashSet<string>();
            excludes.AddRange(
                nameof(model.Address1),
                nameof(model.Address2),
                nameof(model.Address3),
                nameof(model.City),
                nameof(model.County),
                nameof(model.Country),
                nameof(model.Postcode),
                nameof(model.PoBox));

            //Exclude the contact details
            excludes.AddRange(
                nameof(model.ContactFirstName),
                nameof(model.ContactLastName),
                nameof(model.ContactJobTitle),
                nameof(model.ContactEmailAddress),
                nameof(model.ContactPhoneNumber));

            //Exclude the SIC Codes
            excludes.Add(nameof(model.SicCodeIds));

            excludes.Add(nameof(model.SearchText));
            excludes.Add(nameof(model.OrganisationName));
            excludes.AddRange(
                nameof(model.CompanyNumber),
                nameof(model.CharityNumber),
                nameof(model.MutualNumber),
                nameof(model.OtherName),
                nameof(model.OtherValue));

            ModelState.Exclude(excludes.ToArray());

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<OrganisationViewModel>();
                return View(nameof(ReviewRequest), model);
            }

            if (command.EqualsI("decline"))
            {
                result = RedirectToAction("ConfirmCancellation");
            }
            else if (command.EqualsI("approve"))
            {
                Organisation conflictOrg = null;

                //Check for company number conflicts
                if (!string.IsNullOrWhiteSpace(model.CompanyNumber))
                {
                    conflictOrg = await DataRepository.GetAll<Organisation>()
                        .FirstOrDefaultAsync(
                            o => userOrg.OrganisationId != o.OrganisationId && o.CompanyNumber.ToLower() == model.CompanyNumber.ToLower());
                    if (conflictOrg != null)
                    {
                        ModelState.AddModelError(
                            3031,
                            nameof(model.CompanyNumber),
                            new {organisationName = conflictOrg.OrganisationName, referenceName = "Company number"});
                    }
                }

                //Check for charity number conflicts
                if (!string.IsNullOrWhiteSpace(model.CharityNumber))
                {
                    OrganisationReference orgRef = await DataRepository.GetAll<OrganisationReference>()
                        .FirstOrDefaultAsync(
                            o => userOrg.OrganisationId != o.OrganisationId
                                 && o.ReferenceName.ToLower() == nameof(model.CharityNumber).ToLower()
                                 && o.ReferenceValue.ToLower() == model.CharityNumber.ToLower());
                    conflictOrg = orgRef?.Organisation;
                    if (conflictOrg != null)
                    {
                        ModelState.AddModelError(
                            3031,
                            nameof(model.CharityNumber),
                            new {organisationName = conflictOrg.OrganisationName, referenceName = "Charity number"});
                    }
                }

                //Check for mutual number conflicts
                if (!string.IsNullOrWhiteSpace(model.MutualNumber))
                {
                    OrganisationReference orgRef = await DataRepository.GetAll<OrganisationReference>()
                        .FirstOrDefaultAsync(
                            o => userOrg.OrganisationId != o.OrganisationId
                                 && o.ReferenceName.ToLower() == nameof(model.MutualNumber).ToLower()
                                 && o.ReferenceValue.ToLower() == model.MutualNumber.ToLower());
                    conflictOrg = orgRef?.Organisation;
                    if (conflictOrg != null)
                    {
                        ModelState.AddModelError(
                            3031,
                            nameof(model.MutualNumber),
                            new {organisationName = conflictOrg.OrganisationName, referenceName = "Mutual partnership number"});
                    }
                }

                //Check for other reference conflicts
                if (!string.IsNullOrWhiteSpace(model.OtherValue))
                {
                    OrganisationReference orgRef = await DataRepository.GetAll<OrganisationReference>()
                        .FirstOrDefaultAsync(
                            o => userOrg.OrganisationId != o.OrganisationId
                                 && o.ReferenceName.ToLower() == model.OtherName.ToLower()
                                 && o.ReferenceValue.ToLower() == model.OtherValue.ToLower());
                    conflictOrg = orgRef?.Organisation;
                    if (conflictOrg != null)
                    {
                        ModelState.AddModelError(
                            3031,
                            nameof(model.OtherValue),
                            new {organisationName = conflictOrg.OrganisationName, referenceName = model.OtherValue});
                    }
                }

                if (!ModelState.IsValid)
                {
                    this.CleanModelErrors<OrganisationViewModel>();
                    return View("ReviewRequest", model);
                }

                //Activate the org user
                userOrg.PINConfirmedDate = VirtualDateTime.Now;

                //Activate the organisation
                userOrg.Organisation.SetStatus(
                    OrganisationStatuses.Active,
                    OriginalUser == null ? currentUser.UserId : OriginalUser.UserId,
                    "Manually registered");

                // save any sic codes
                if (!string.IsNullOrEmpty(model.SicCodeIds))
                {
                    IOrderedEnumerable<int> newSicCodes = model.SicCodeIds.Split(',').Cast<int>().OrderBy(sc => sc);
                    foreach (int sc in newSicCodes)
                    {
                        userOrg.Organisation.OrganisationSicCodes.Add(
                            new OrganisationSicCode
                            {
                                SicCodeId = sc, OrganisationId = userOrg.OrganisationId, Created = VirtualDateTime.Now
                            });
                    }
                }

                //Retire the old address 
                OrganisationAddress latestAddress = userOrg.Organisation.GetLatestAddress();
                if (latestAddress != null && latestAddress.AddressId != userOrg.Address.AddressId)
                {
                    latestAddress.SetStatus(
                        AddressStatuses.Retired,
                        OriginalUser == null ? currentUser.UserId : OriginalUser.UserId,
                        "Replaced by Manual registration");
                }

                //Activate the address
                userOrg.Address.SetStatus(
                    AddressStatuses.Active,
                    OriginalUser == null ? currentUser.UserId : OriginalUser.UserId,
                    "Manually registered");

                //Send the approved email to the applicant
                SendRegistrationAccepted(
                    userOrg.User.ContactEmailAddress.Coalesce(userOrg.User.EmailAddress),
                    userOrg.User.EmailAddress.StartsWithI(Global.TestPrefix));

                //Log the approval
                if (!userOrg.User.EmailAddress.StartsWithI(Global.TestPrefix))
                {
                    await Global.RegistrationLog.WriteAsync(
                        new RegisterLogModel
                        {
                            StatusDate = VirtualDateTime.Now,
                            Status = "Manually registered",
                            ActionBy = currentUser.EmailAddress,
                            Details = "",
                            Sector = userOrg.Organisation.SectorType,
                            Organisation = userOrg.Organisation.OrganisationName,
                            CompanyNo = userOrg.Organisation.CompanyNumber,
                            Address = userOrg?.Address.GetAddressString(),
                            SicCodes = userOrg.Organisation.GetSicCodeIdsString(),
                            UserFirstname = userOrg.User.Firstname,
                            UserLastname = userOrg.User.Lastname,
                            UserJobtitle = userOrg.User.JobTitle,
                            UserEmail = userOrg.User.EmailAddress,
                            ContactFirstName = userOrg.User.ContactFirstName,
                            ContactLastName = userOrg.User.ContactLastName,
                            ContactJobTitle = userOrg.User.ContactJobTitle,
                            ContactOrganisation = userOrg.User.ContactOrganisation,
                            ContactPhoneNumber = userOrg.User.ContactPhoneNumber
                        });
                }

                //Show confirmation
                if (currentUser.EmailAddress.StartsWithI(Global.TestPrefix))
                {
                    TempData["TestUrl"] = Url.Action("Impersonate", "Admin", new {emailAddress = userOrg.User.EmailAddress});
                }

                result = RedirectToAction("RequestAccepted");
            }
            else
            {
                return new HttpBadRequestResult($"Invalid command on '{command}'");
            }

            //Save the changes and redirect
            await DataRepository.SaveChangesAsync();

            //Send notification email to existing users 
            EmailSendingServiceHelpers.SendUserAddedEmailToExistingUsers(userOrg.Organisation, userOrg.User, emailSendingService);

            //Ensure the organisation has an employer reference
            if (userOrg.PINConfirmedDate.HasValue && string.IsNullOrWhiteSpace(userOrg.Organisation.EmployerReference))
            {
                await OrganisationBusinessLogic.SetUniqueEmployerReferenceAsync(userOrg.Organisation);
            }

            //Add or remove this organisation to/from the search index
            await SearchBusinessLogic.UpdateSearchIndexAsync(userOrg.Organisation);

            //Save the model for the redirect
            this.StashModel(model);

            return result;
        }

        //Send the registration request
        protected void SendRegistrationAccepted(string emailAddress, bool test = false)
        {
            //Always use the administrators email when not on production
            if (!Config.IsProduction())
            {
                emailAddress = CurrentUser.EmailAddress;
            }

            //Send an acceptance link to the email address
            string returnUrl = Url.Action(nameof(OrganisationController.ManageOrganisations), "Organisation", null, "https");
            emailSendingService.SendOrganisationRegistrationApprovedEmail(emailAddress, returnUrl);
        }

        /// <summary>
        ///     ask the reviewer for decline reason and confirmation ///
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "GPGadmin")]
        [HttpGet("confirm-cancellation")]
        public IActionResult ConfirmCancellation()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Make sure we can load employers from session
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            return View("ConfirmCancellation", model);
        }

        /// <summary>
        ///     On confirmation save the organisation
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "GPGadmin")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("confirm-cancellation")]
        public async Task<IActionResult> ConfirmCancellation(OrganisationViewModel model, string command)
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Load the employers from session
            var m = this.UnstashModel<OrganisationViewModel>();
            if (m == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            //If cancel button clicked the n return to review page
            if (command.EqualsI("Cancel"))
            {
                return RedirectToAction("ReviewRequest");
            }

            //Unwrap code
            UserOrganisation userOrg;
            ActionResult result = UnwrapRegistrationRequest(model, out userOrg);
            if (result != null)
            {
                return result;
            }

            //Log the rejection
            if (!userOrg.User.EmailAddress.StartsWithI(Global.TestPrefix))
            {
                await Global.RegistrationLog.WriteAsync(
                    new RegisterLogModel
                    {
                        StatusDate = VirtualDateTime.Now,
                        Status = "Manually Rejected",
                        ActionBy = currentUser.EmailAddress,
                        Details = "",
                        Sector = userOrg.Organisation.SectorType,
                        Organisation = userOrg.Organisation.OrganisationName,
                        CompanyNo = userOrg.Organisation.CompanyNumber,
                        Address = userOrg?.Address.GetAddressString(),
                        SicCodes = userOrg.Organisation.GetSicCodeIdsString(),
                        UserFirstname = userOrg.User.Firstname,
                        UserLastname = userOrg.User.Lastname,
                        UserJobtitle = userOrg.User.JobTitle,
                        UserEmail = userOrg.User.EmailAddress,
                        ContactFirstName = userOrg.User.ContactFirstName,
                        ContactLastName = userOrg.User.ContactLastName,
                        ContactJobTitle = userOrg.User.ContactJobTitle,
                        ContactOrganisation = userOrg.User.ContactOrganisation,
                        ContactPhoneNumber = userOrg.User.ContactPhoneNumber
                    });
            }

            //Delete address for this user and organisation
            if (userOrg.Address.Status != AddressStatuses.Active && userOrg.Address.CreatedByUserId == userOrg.UserId)
            {
                DataRepository.Delete(userOrg.Address);
            }

            //Delete the org user
            long orgId = userOrg.OrganisationId;
            string emailAddress = userOrg.User.ContactEmailAddress.Coalesce(userOrg.User.EmailAddress);

            //Delete the organisation if it has no returns, is not in scopes table, and is not registered to another user
            if (userOrg.Organisation != null
                && !userOrg.Organisation.Returns.Any()
                && !userOrg.Organisation.OrganisationScopes.Any()
                && !await DataRepository.GetAll<UserOrganisation>()
                    .AnyAsync(uo => uo.OrganisationId == userOrg.Organisation.OrganisationId && uo.UserId != userOrg.UserId))
            {
                _logger.LogInformation(
                    $"Unused organisation {userOrg.OrganisationId}:'{userOrg.Organisation.OrganisationName}' deleted by {(OriginalUser == null ? currentUser.EmailAddress : OriginalUser.EmailAddress)} when declining manual registration for {userOrg.User.EmailAddress}");
                DataRepository.Delete(userOrg.Organisation);
            }

            EmployerSearchModel searchRecord = userOrg.Organisation.ToEmployerSearchResult(true);
            DataRepository.Delete(userOrg);

            //Send the declined email to the applicant
            SendRegistrationDeclined(
                emailAddress,
                string.IsNullOrWhiteSpace(model.CancellationReason)
                    ? "We haven't been able to verify your employer's identity. So we have declined your application."
                    : model.CancellationReason);

            //Save the changes and redirect
            await DataRepository.SaveChangesAsync();

            //Remove this organisation from the search index
            await Global.SearchRepository.RemoveFromIndexAsync(new[] {searchRecord});

            //Save the model for the redirect
            this.StashModel(model);

            //If private sector then send the pin
            return RedirectToAction("RequestCancelled");
        }


        //Send the registration request
        protected void SendRegistrationDeclined(string emailAddress, string reason)
        {
            //Always use the administrators email when not on production
            if (!Config.IsProduction())
            {
                emailAddress = CurrentUser.EmailAddress;
            }

            //Send a verification link to the email address
            emailSendingService.SendOrganisationRegistrationDeclinedEmail(emailAddress, reason);
        }

        /// <summary>
        ///     Show review accepted confirmation
        ///     <returns></returns>
        [Authorize(Roles = "GPGadmin")]
        [HttpGet("request-accepted")]
        public IActionResult RequestAccepted()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Make sure we can load model from session
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            //Clear the stash
            this.ClearStash();

            if (currentUser.EmailAddress.StartsWithI(Global.TestPrefix) && TempData.ContainsKey("TestUrl"))
            {
                ViewBag.TestUrl = TempData["TestUrl"];
            }

            return View("RequestAccepted", model);
        }

        /// <summary>
        ///     Show review cancel confirmation
        ///     <returns></returns>
        [Authorize(Roles = "GPGadmin")]
        [HttpGet("request-cancelled")]
        public IActionResult RequestCancelled()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Make sure we can load model from session
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            //Clear the stash
            this.ClearStash();

            if (currentUser.EmailAddress.StartsWithI(Global.TestPrefix))
            {
                UserOrganisation userOrg;
                ActionResult result = UnwrapRegistrationRequest(model, out userOrg);

                ViewBag.TestUrl = Url.Action("Impersonate", "Admin", new {emailAddress = userOrg.User.EmailAddress});
            }

            return View("RequestCancelled", model);
        }

        #endregion

    }
}
