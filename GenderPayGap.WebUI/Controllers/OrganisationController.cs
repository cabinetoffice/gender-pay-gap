using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.BusinessLogic.Services;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Organisation;
using GenderPayGap.WebUI.Repositories;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{

    public partial class OrganisationController : BaseController
    {

        private readonly EmailSendingService emailSendingService;

        #region Constructors

        public OrganisationController(
            IHttpCache cache,
            IHttpSession session,
            ISubmissionService submitService,
            IScopeBusinessLogic scopeBL,
            IDataRepository dataRepository,
            RegistrationRepository registrationRepository,
            IWebTracker webTracker,
            EmailSendingService emailSendingService) : base(
            cache,
            session,
            dataRepository,
            webTracker)
        {
            SubmissionService = submitService;
            ScopeBusinessLogic = scopeBL;
            RegistrationRepository = registrationRepository;
            this.emailSendingService = emailSendingService;
        }

        #endregion

        [Authorize]
        [HttpGet("~/declare-scope/{id}")]
        public IActionResult DeclareScope(string id)
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
                return new HttpBadRequestResult($"Cannot decrypt employer id {id}");
            }

            // Check the user has permission for this organisation
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for employer id {organisationId}");
            }

            // Ensure this user is registered fully for this organisation
            if (userOrg.PINConfirmedDate == null)
            {
                return new HttpForbiddenResult(
                    $"User {currentUser?.EmailAddress} has not completed registration for employer {userOrg.Organisation.EmployerReference}");
            }

            //Get the current snapshot date
            DateTime snapshotDate = userOrg.Organisation.SectorType.GetAccountingStartDate().AddYears(-1);
            if (snapshotDate.Year < Global.FirstReportingYear)
            {
                return new HttpBadRequestResult($"Snapshot year {snapshotDate.Year} is invalid");
            }

            ScopeStatuses scopeStatus =
                ScopeBusinessLogic.GetLatestScopeStatusForSnapshotYear(organisationId, snapshotDate.Year);
            if (scopeStatus.IsAny(ScopeStatuses.InScope, ScopeStatuses.OutOfScope))
            {
                return new HttpBadRequestResult("Explicit scope is already set");
            }

            // build the view model
            var model = new DeclareScopeModel { OrganisationId = userOrg.OrganisationId, OrganisationName = userOrg.Organisation.OrganisationName, SnapshotDate = snapshotDate };

            return View(model);
        }

        [PreventDuplicatePost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("~/declare-scope/{id}")]
        public IActionResult DeclareScope(DeclareScopeModel model, string id)
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
                return new HttpBadRequestResult($"Cannot decrypt employer id {id}");
            }


            // Check the user has permission for this organisation
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for employer id {organisationId}");
            }

            // Ensure this user is registered fully for this organisation
            if (userOrg.PINConfirmedDate == null)
            {
                return new HttpForbiddenResult(
                    $"User {currentUser?.EmailAddress} has not completed registration for employer {userOrg.Organisation.EmployerReference}");
            }

            //Check the year parameters
            if (model.SnapshotDate.Year < Global.FirstReportingYear || model.SnapshotDate.Year > VirtualDateTime.Now.Year)
            {
                return new HttpBadRequestResult($"Snapshot year {model.SnapshotDate.Year} is invalid");
            }

            //Check if we need the current years scope
            ScopeStatuses scopeStatus =
                ScopeBusinessLogic.GetLatestScopeStatusForSnapshotYear(organisationId, model.SnapshotDate.Year);
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
            var newScope = new OrganisationScope
            {
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
            ScopeBusinessLogic.SaveScope(userOrg.Organisation, true, newScope);
            return View("ScopeDeclared", model);
        }

        [Authorize]
        [HttpGet("~/scope-declared/{id}")]
        public IActionResult ScopeDeclared(string id)
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
                return new HttpBadRequestResult($"Cannot decrypt employer id {id}");
            }

            // Check the user has permission for this organisation
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for employer id {organisationId}");
            }

            // Ensure this user is registered fully for this organisation
            if (userOrg.PINConfirmedDate == null)
            {
                return new HttpForbiddenResult(
                    $"User {currentUser?.EmailAddress} has not completed registration for employer {userOrg.Organisation.EmployerReference}");
            }

            //Get the current snapshot date
            DateTime snapshotDate = userOrg.Organisation.SectorType.GetAccountingStartDate().AddYears(-1);
            if (snapshotDate.Year < Global.FirstReportingYear)
            {
                return new HttpBadRequestResult($"Snapshot year {snapshotDate.Year} is invalid");
            }

            ScopeStatuses scopeStatus =
                ScopeBusinessLogic.GetLatestScopeStatusForSnapshotYear(organisationId, snapshotDate.Year);

            var model = new DeclareScopeModel
            {
                OrganisationId = userOrg.OrganisationId, 
                OrganisationName = userOrg.Organisation.OrganisationName, 
                SnapshotDate = snapshotDate, 
                ScopeStatus = scopeStatus
            };

            return View("ScopeDeclared", model);
        }

        [Authorize]
        [HttpGet("~/activate-organisation/{id}")]
        public IActionResult ActivateOrganisation(string id)
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
                return new HttpBadRequestResult($"Cannot decrypt employe id {id}");
            }

            // Check the user has permission for this organisation
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for employer id {organisationId}");
            }

            // TODO - Delete this once PITP is enabled
            if (userOrg.HasExpiredPin())
            {
                userOrg.Organisation.UserOrganisations.Remove(userOrg);
                DataRepository.Delete(userOrg);
                DataRepository.SaveChanges();

                throw new PinExpiredException();
            }

            // Ensure this organisation needs activation on the users account
            if (userOrg.HasBeenActivated())
            {
                throw new Exception(
                    $"Attempt to activate employer {userOrg.OrganisationId}:'{userOrg.Organisation.OrganisationName}' for {currentUser.EmailAddress} by '{(OriginalUser == null ? currentUser.EmailAddress : OriginalUser.EmailAddress)}' which has already been activated");
            }

            // begin ActivateService journey
            ReportingOrganisationId = organisationId;
            return RedirectToAction("ActivateService", "Register");
        }

        #region Dependencies

        public ISubmissionService SubmissionService { get; }

        public IScopeBusinessLogic ScopeBusinessLogic { get; }

        public RegistrationRepository RegistrationRepository { get; }

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
                return new HttpBadRequestResult($"Cannot decrypt employer id {orgId}");
            }

            // Check the current user has remove permission for this organisation
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for employer id {organisationId}");
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
                    return new HttpForbiddenResult($"User {userIdToRemove} is not registered for employer id {organisationId}");
                }

                userToRemove = otherUserOrg.User;
            }

            //Make sure they are fully registered for one before requesting another
            if (userOrg.IsAwaitingActivationPIN())
            {
                TimeSpan remainingTime = userOrg.PINSentDate.Value.AddMinutes(Global.LockoutMinutes) - VirtualDateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                {
                    return View("CustomError", new ErrorViewModel(3023, new { remainingTime = remainingTime.ToFriendly(maxParts: 2) }));
                }
            }

            // build the view model
            var model = new RemoveOrganisationModel
            {
                EncOrganisationId = orgId,
                EncUserId = userId,
                OrganisationName = userOrg.Organisation.OrganisationName,
                OrganisationAddress = userOrg.Organisation.GetLatestAddress().GetAddressLines(),
                UserName = userToRemove.Fullname
            };

            //Return the confirmation page
            return View("ConfirmRemove", model);
        }

        [PreventDuplicatePost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("~/remove-organisation")]
        public IActionResult RemoveOrganisation(RemoveOrganisationModel model)
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
                return new HttpBadRequestResult($"Cannot decrypt employer id {model.EncOrganisationId}");
            }

            // Check the current user has permission for this organisation
            UserOrganisation userOrgToUnregister = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrgToUnregister == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for employer id {organisationId}");
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
                    return new HttpForbiddenResult($"User {userIdToRemove} is not registered for employer id {organisationId}");
                }

                userToUnregister = otherUserOrg.User;
                userOrgToUnregister = otherUserOrg;
            }

            // Remove the registration
            User actionByUser = IsImpersonatingUser == false ? currentUser : OriginalUser;
            Organisation orgToRemove = userOrgToUnregister.Organisation;
            RegistrationRepository.RemoveRegistration(userOrgToUnregister, actionByUser);

            // Email user that has been unregistered
            emailSendingService.SendRemovedUserFromOrganisationEmail(
                userToUnregister.EmailAddress,
                orgToRemove.OrganisationName,
                userToUnregister.Fullname);

            // Email the other users of the organisation
            IEnumerable<string> emailAddressesForOrganisation = orgToRemove.UserOrganisations.Select(uo => uo.User.EmailAddress);
            foreach (string emailAddress in emailAddressesForOrganisation)
            {
                emailSendingService.SendRemovedUserFromOrganisationEmail(
                    emailAddress,
                    orgToRemove.OrganisationName,
                    userToUnregister.Fullname);
            }

            // Send the notification to GEO for each newly orphaned organisation
            if (orgToRemove.GetIsOrphan())
            {
                emailSendingService.SendGeoOrphanOrganisationEmail(orgToRemove.OrganisationName);
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
