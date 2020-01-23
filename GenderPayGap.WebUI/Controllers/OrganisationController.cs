using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using GenderPayGap.BusinessLogic;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.Models.Organisation;
using GenderPayGap.WebUI.Models.Scope;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebUI.Controllers
{

    public partial class OrganisationController : BaseController
    {

        #region Constructors

        public OrganisationController(
            ILogger<ErrorController> logger,
            IHttpCache cache,
            IHttpSession session,
            ISubmissionService submitService,
            IScopePresentation scopePresentation,
            IScopeBusinessLogic scopeBL,
            IOrganisationBusinessLogic organisationBL,
            IDataRepository dataRepository,
            IRegistrationRepository registrationRepository,
            IWebTracker webTracker,
            [KeyFilter("Private")] IPagedRepository<EmployerRecord> privateSectorRepository,
            [KeyFilter("Public")] IPagedRepository<EmployerRecord> publicSectorRepository) : base(
            logger,
            cache,
            session,
            dataRepository,
            webTracker)
        {
            SubmissionService = submitService;
            ScopePresentation = scopePresentation;
            ScopeBusinessLogic = scopeBL;
            OrganisationBusinessLogic = organisationBL;
            PrivateSectorRepository = privateSectorRepository;
            PublicSectorRepository = publicSectorRepository;
            RegistrationRepository = registrationRepository;
        }

        #endregion

        [Authorize]
        [HttpGet("~/declare-scope/{id}")]
        public async Task<IActionResult> DeclareScope(string id)
        {
            //Ensure user has completed the registration process
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // Decrypt org id
            if (!id.DecryptToId(out long organisationId))
            {
                return new HttpBadRequestResult($"Cannot decrypt organisation id {id}");
            }

            // Check the user has permission for this organisation
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for organisation id {organisationId}");
            }

            // Ensure this user is registered fully for this organisation
            if (userOrg.PINConfirmedDate == null)
            {
                return new HttpForbiddenResult(
                    $"User {currentUser?.EmailAddress} has not completed registration for organisation {userOrg.Organisation.EmployerReference}");
            }

            //Get the current snapshot date
            DateTime snapshotDate = SubmissionService.GetCurrentSnapshotDate(userOrg.Organisation.SectorType).AddYears(-1);
            if (snapshotDate.Year < Global.FirstReportingYear)
            {
                return new HttpBadRequestResult($"Snapshot year {snapshotDate.Year} is invalid");
            }

            ScopeStatuses scopeStatus =
                await ScopeBusinessLogic.GetLatestScopeStatusForSnapshotYearAsync(organisationId, snapshotDate.Year);
            if (scopeStatus.IsAny(ScopeStatuses.InScope, ScopeStatuses.OutOfScope))
            {
                return new HttpBadRequestResult("Explicit scope is already set");
            }

            // build the view model
            var model = new DeclareScopeModel {OrganisationName = userOrg.Organisation.OrganisationName, SnapshotDate = snapshotDate};

            return View(model);
        }

        [PreventDuplicatePost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("~/declare-scope/{id}")]
        public async Task<IActionResult> DeclareScope(DeclareScopeModel model, string id)
        {
            // Ensure user has completed the registration process
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // Decrypt org id
            if (!id.DecryptToId(out long organisationId))
            {
                return new HttpBadRequestResult($"Cannot decrypt organisation id {id}");
            }


            // Check the user has permission for this organisation
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for organisation id {organisationId}");
            }

            // Ensure this user is registered fully for this organisation
            if (userOrg.PINConfirmedDate == null)
            {
                return new HttpForbiddenResult(
                    $"User {currentUser?.EmailAddress} has not completed registeration for organisation {userOrg.Organisation.EmployerReference}");
            }

            //Check the year parameters
            if (model.SnapshotDate.Year < Global.FirstReportingYear || model.SnapshotDate.Year > VirtualDateTime.Now.Year)
            {
                return new HttpBadRequestResult($"Snapshot year {model.SnapshotDate.Year} is invalid");
            }

            //Check if we need the current years scope
            ScopeStatuses scopeStatus =
                await ScopeBusinessLogic.GetLatestScopeStatusForSnapshotYearAsync(organisationId, model.SnapshotDate.Year);
            if (scopeStatus.IsAny(ScopeStatuses.InScope, ScopeStatuses.OutOfScope))
            {
                return new HttpBadRequestResult("Explicit scope is already set");
            }

            //Validate the submitted fields
            ModelState.Clear();

            if (model.ScopeStatus == null || model.ScopeStatus == ScopeStatuses.Unknown)
            {
                AddModelError(3032, "ScopeStatus");
            }

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<DeclareScopeModel>();
                return View("DeclareScope", model);
            }

            //Create last years declared scope
            var newScope = new OrganisationScope {
                OrganisationId = userOrg.OrganisationId,
                Organisation = userOrg.Organisation,
                ContactEmailAddress = currentUser.EmailAddress,
                ContactFirstname = currentUser.Firstname,
                ContactLastname = currentUser.Lastname,
                ScopeStatus = model.ScopeStatus.Value,
                Status = ScopeRowStatuses.Active,
                ScopeStatusDate = VirtualDateTime.Now,
                SnapshotDate = model.SnapshotDate
            };

            //Save the new declared scopes
            await ScopeBusinessLogic.SaveScopeAsync(userOrg.Organisation, true, newScope);
            return View("ScopeDeclared", model);
        }

        [Authorize]
        [HttpGet("~/activate-organisation/{id}")]
        public IActionResult ActivateOrganisation(string id)
        {
            //Ensure user has completed the registration process
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // Decrypt org id
            if (!id.DecryptToId(out long organisationId))
            {
                return new HttpBadRequestResult($"Cannot decrypt organisation id {id}");
            }

            // Check the user has permission for this organisation
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for organisation id {organisationId}");
            }

            // Ensure this organisation needs activation on the users account
            if (userOrg.PINConfirmedDate != null)
            {
                throw new Exception(
                    $"Attempt to activate organisation {userOrg.OrganisationId}:'{userOrg.Organisation.OrganisationName}' for {currentUser.EmailAddress} by '{(OriginalUser == null ? currentUser.EmailAddress : OriginalUser.EmailAddress)}' which has already been activated");
            }

            // begin ActivateService journey
            ReportingOrganisationId = organisationId;
            return RedirectToAction("ActivateService", "Register");
        }

        [Authorize]
        [HttpGet("~/report-for-organisation/{request}")]
        public async Task<IActionResult> ReportForOrganisation(string request)
        {
            //Ensure user has completed the registration process
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // Decrypt request
            if (!request.DecryptToParams(out List<string> requestParams))
            {
                return new HttpBadRequestResult($"Cannot decrypt parameters '{request}'");
            }

            // Extract the request vars
            long organisationId = requestParams[0].ToInt64();
            int reportingStartYear = requestParams[1].ToInt32();
            bool change = requestParams[2].ToBoolean();

            // Ensure we can report for the year requested
            if (!SubmissionService.IsValidSnapshotYear(reportingStartYear))
            {
                return new HttpBadRequestResult($"Invalid snapshot year {reportingStartYear}");
            }

            // Check the user has permission for this organisation
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for organisation id {organisationId}");
            }

            // get the sector
            SectorTypes sectorType = userOrg.Organisation.SectorType;

            // Determine if this is for the previous reporting year
            bool isPrevReportingYear = SubmissionService.IsCurrentSnapshotYear(sectorType, reportingStartYear) == false;

            // Set the reporting session globals
            ReportingOrganisationId = organisationId;
            ReportingOrganisationStartYear = reportingStartYear;

            // Clear the SubmitController stash
            this.ClearAllStashes();

            await SubmissionService.RestartDraftFileAsync(organisationId, reportingStartYear, currentUser.UserId);

            // When previous reporting year then do late submission flow
            if (isPrevReportingYear)
            {
                // Change an existing late submission
                if (change)
                {
                    return RedirectToAction("LateWarning", "Submit", new {request, returnUrl = "CheckData"});
                }

                // Create new a late submission
                return RedirectToAction("LateWarning", "Submit", new {request});
            }

            /*
 * Under normal circumstances, we might want to stash the model at this point, just before the redirection, however, we are NOT going to for two reasons:
 *      (1) The information currently on the model includes ONLY the bare minimum to know if there is a draft or not, it doesn't for example, include anything to do with the permissions to access, who is locked it, lastWrittenTimestamp... This behaviour is by design: the draft file is locked on access, and that will happen once the user arrives to 'check data' or 'enter calculations', if we were to stash the model now, the stashed info won't contain all relevant draft information.
 *      (2) Currently stash/unstash only works with the name of the controller, so it really doesn't matter what we stash here, the 'check data' and 'enter calculations' page belong to a different controller, so the stashed info will never be read by them anyway.
 */
            // Change an existing submission
            if (change)
            {
                return RedirectToAction("CheckData", "Submit");
            }

            // Create new a submission
            return RedirectToAction("EnterCalculations", "Submit");
        }

        [Authorize]
        [HttpGet("~/change-organisation-scope/{request}")]
        public IActionResult ChangeOrganisationScope(string request)
        {
            // Ensure user has completed the registration process
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // Decrypt request
            if (!request.DecryptToParams(out List<string> requestParams))
            {
                return new HttpBadRequestResult($"Cannot decrypt request parameters '{request}'");
            }

            // Extract the request vars
            long organisationId = requestParams[0].ToInt64();
            int reportingStartYear = requestParams[1].ToInt32();

            // Check the user has permission for this organisation
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for organisation id {organisationId}");
            }

            // Generate the scope state model
            ScopingViewModel stateModel = ScopePresentation.CreateScopingViewModel(userOrg.Organisation, CurrentUser);

            // Get the latest scope for the reporting year
            ScopeViewModel latestScope = stateModel.ThisScope.SnapshotDate.Year == reportingStartYear ? stateModel.ThisScope :
                stateModel.LastScope.SnapshotDate.Year == reportingStartYear ? stateModel.LastScope : null;

            // Set the return url
            stateModel.StartUrl = Url.Action("ManageOrganisation", new {id = Encryption.EncryptQuerystring(organisationId.ToString())});
            stateModel.IsChangeJourney = true;
            stateModel.AccountingDate = latestScope.SnapshotDate;

            //Set the in/out journey type
            stateModel.IsOutOfScopeJourney = latestScope.ScopeStatus.IsAny(ScopeStatuses.PresumedInScope, ScopeStatuses.InScope);

            // Stash the model for the scope controller
            this.StashModel(typeof(ScopeController), stateModel);

            if (stateModel.IsOutOfScopeJourney)
            {
                return RedirectToAction("EnterOutOfScopeAnswers", "Scope");
            }

            return RedirectToAction("ConfirmInScope", "Scope");
        }

        #region Dependencies

        public ISubmissionService SubmissionService { get; }

        public IScopePresentation ScopePresentation { get; }

        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; }

        public IScopeBusinessLogic ScopeBusinessLogic { get; }

        public IPagedRepository<EmployerRecord> PrivateSectorRepository { get; }

        public IPagedRepository<EmployerRecord> PublicSectorRepository { get; }

        public IRegistrationRepository RegistrationRepository { get; }

        #endregion

        #region RemoveOrganisation

        [Authorize]
        [HttpGet("~/remove-organisation")]
        public IActionResult RemoveOrganisation(string orgId, string userId)
        {
            //Ensure user has completed the registration process
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // Decrypt org id
            if (!orgId.DecryptToId(out long organisationId))
            {
                return new HttpBadRequestResult($"Cannot decrypt organisation id {orgId}");
            }

            // Check the current user has remove permission for this organisation
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for organisation id {organisationId}");
            }

            // Decrypt user id
            if (!userId.DecryptToId(out long userIdToRemove))
            {
                return new HttpBadRequestResult($"Cannot decrypt user id {userId}");
            }

            User userToRemove = currentUser;
            if (currentUser.UserId != userIdToRemove)
            {
                // Check the other user has permission to see this organisation
                UserOrganisation otherUserOrg =
                    userOrg.Organisation.UserOrganisations.FirstOrDefault(
                        uo => uo.OrganisationId == organisationId && uo.UserId == userIdToRemove);
                if (otherUserOrg == null)
                {
                    return new HttpForbiddenResult($"User {userIdToRemove} is not registered for organisation id {organisationId}");
                }

                userToRemove = otherUserOrg.User;
            }

            //Make sure they are fully registered for one before requesting another
            if (userOrg.PINConfirmedDate == null && userOrg.PINSentDate != null)
            {
                TimeSpan remainingTime = userOrg.PINSentDate.Value.AddMinutes(Global.LockoutMinutes) - VirtualDateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                {
                    return View("CustomError", new ErrorViewModel(3023, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
                }
            }

            // build the view model
            var model = new RemoveOrganisationModel {
                EncOrganisationId = orgId,
                EncUserId = userId,
                OrganisationName = userOrg.Organisation.OrganisationName,
                OrganisationAddress = userOrg.Organisation.LatestAddress.GetAddressString(),
                UserName = userToRemove.Fullname
            };

            //Return the confirmation page
            return View("ConfirmRemove", model);
        }

        [PreventDuplicatePost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("~/remove-organisation")]
        public async Task<IActionResult> RemoveOrganisation(RemoveOrganisationModel model)
        {
            // Ensure user has completed the registration process
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // Decrypt org id
            if (!model.EncOrganisationId.DecryptToId(out long organisationId))
            {
                return new HttpBadRequestResult($"Cannot decrypt organisation id {model.EncOrganisationId}");
            }

            // Check the current user has permission for this organisation
            UserOrganisation userOrgToUnregister = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrgToUnregister == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for organisation id {organisationId}");
            }

            // Decrypt user id
            if (!model.EncUserId.DecryptToId(out long userIdToRemove))
            {
                return new HttpBadRequestResult($"Cannot decrypt user id '{model.EncUserId}'");
            }

            Organisation sourceOrg = userOrgToUnregister.Organisation;
            User userToUnregister = currentUser;
            if (currentUser.UserId != userIdToRemove)
            {
                // Ensure the other user has registered this organisation
                UserOrganisation otherUserOrg =
                    sourceOrg.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId && uo.UserId == userIdToRemove);
                if (otherUserOrg == null)
                {
                    return new HttpForbiddenResult($"User {userIdToRemove} is not registered for organisation id {organisationId}");
                }

                userToUnregister = otherUserOrg.User;
                userOrgToUnregister = otherUserOrg;
            }

            // Remove the registration
            User actionByUser = IsImpersonatingUser == false ? currentUser : OriginalUser;
            Organisation orgToRemove = userOrgToUnregister.Organisation;
            await RegistrationRepository.RemoveRegistrationAsync(userOrgToUnregister, actionByUser);

            // Email user that has been unregistered
            EmailSendingService.SendRemovedUserFromOrganisationEmail(
                userToUnregister.EmailAddress,
                orgToRemove.OrganisationName,
                userToUnregister.Fullname);

            // Email the other users of the organisation
            IEnumerable<string> emailAddressesForOrganisation = orgToRemove.UserOrganisations.Select(uo => uo.User.EmailAddress);
            foreach (string emailAddress in emailAddressesForOrganisation)
            {
                EmailSendingService.SendRemovedUserFromOrganisationEmail(
                    emailAddress,
                    orgToRemove.OrganisationName,
                    userToUnregister.Fullname);
            }

            // Send the notification to GEO for each newly orphaned organisation
            if (!userToUnregister.EmailAddress.StartsWithI(Global.TestPrefix))
            {
                var sendEmails = new List<Task>();
                bool testEmail = !Config.IsProduction();
                if (orgToRemove.GetIsOrphan())
                {
                    sendEmails.Add(Emails.SendGEOOrphanOrganisationNotificationAsync(orgToRemove.OrganisationName, testEmail));
                }

                await Task.WhenAll(sendEmails);
            }

            //Make sure this organisation is no longer selected
            if (ReportingOrganisationId == organisationId)
            {
                ReportingOrganisationId = 0;
            }

            this.StashModel(model);

            return RedirectToAction("RemoveOrganisationCompleted");
        }

        [Authorize]
        [HttpGet("~/remove-organisation-completed")]
        public IActionResult RemoveOrganisationCompleted()
        {
            // Unstash and clear the remove model
            var model = this.UnstashModel<RemoveOrganisationModel>(typeof(OrganisationController), true);

            // When model is null then return session expired view
            if (model == null)
            {
                return SessionExpiredView();
            }

            return View(model);
        }

        #endregion

    }
}
