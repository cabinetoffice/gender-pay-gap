using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.AddOrganisation;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Search;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminPendingRegistrationsController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly AddOrganisationSearchService addOrganisationSearchService;
        private readonly EmailSendingService emailSendingService;
        private readonly AuditLogger auditLogger;

        public AdminPendingRegistrationsController(
            IDataRepository dataRepository,
            AddOrganisationSearchService addOrganisationSearchService,
            EmailSendingService emailSendingService,
            AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.addOrganisationSearchService = addOrganisationSearchService;
            this.emailSendingService = emailSendingService;
            this.auditLogger = auditLogger;
        }

        [HttpGet("pending-registrations")]
        public IActionResult PendingRegistrations()
        {
            List<UserOrganisation> allManualRegistrations =
                dataRepository
                    .GetAll<UserOrganisation>()
                    .Where(uo => uo.User.Status == UserStatuses.Active)
                    .Where(uo => uo.PINConfirmedDate == null)
                    .Where(uo => uo.Method == RegistrationMethods.Manual)
                    .OrderBy(uo => uo.Modified)
                    .ToList();

            List<UserOrganisation> nonUkAddressRegistrations = allManualRegistrations.Where(uo => uo.Organisation.GetLatestAddress().IsUkAddress == false).ToList();
            List<UserOrganisation> publicSectorRegistrations = allManualRegistrations.Where(uo => uo.Organisation.SectorType == SectorTypes.Public).ToList();
            List<UserOrganisation> remainingRegistrations = allManualRegistrations.Except(publicSectorRegistrations).Except(nonUkAddressRegistrations).ToList();

            var model = new PendingRegistrationsViewModel {
                PublicSectorUserOrganisations = publicSectorRegistrations,
                NonUkAddressUserOrganisations = nonUkAddressRegistrations,
                ManuallyRegisteredUserOrganisations = remainingRegistrations
            };

            return View("PendingRegistrations", model);
        }

        [HttpGet("pending-registrations/{userId}/{organisationId}")] 
        public IActionResult PendingRegistrationGet(long userId, long organisationId) 
        {
            UserOrganisation userOrganisation = LoadUserOrganisationOrThrowError(userId, organisationId);

            var viewModel = new AdminPendingRegistrationViewModel();
            PopulateViewModelFromUserOrganisation(viewModel, userOrganisation);

            return View("PendingRegistration", viewModel); 
        }

        [ValidateAntiForgeryToken]
        [HttpPost("pending-registrations/{userId}/{organisationId}")] 
        public IActionResult PendingRegistrationPost(long userId, long organisationId, AdminPendingRegistrationViewModel viewModel) 
        {
            UserOrganisation userOrganisation = LoadUserOrganisationOrThrowError(userId, organisationId);

            if (ModelState.IsValid)
            {
                switch (viewModel.ApproveOrReject)
                {
                    case AdminPendingRegistrationApproveOrReject.Approve:
                        ApproveRegistration(userOrganisation);
                        return View("PendingRegistrationApproved", userOrganisation);
                    
                    case AdminPendingRegistrationApproveOrReject.Reject:
                        // It is important to generate the page BEFORE we make changes to the database.
                        // If we save changes first, the UserOrganisation will be un-linked from its User and Organisation
                        //   and the page won't render correctly!
                        ViewResult responsePage = View("PendingRegistrationRejected", userOrganisation);
                        
                        RejectRegistration(userOrganisation, viewModel);
                        return responsePage;

                    case null:
                    default:
                        break; // Drop through to the error state below
                }
            }

            PopulateViewModelFromUserOrganisation(viewModel, userOrganisation);
            return View("PendingRegistration", viewModel);
        }

        private UserOrganisation LoadUserOrganisationOrThrowError(long userId, long organisationId)
        {
            UserOrganisation userOrganisation = dataRepository
                .GetAll<UserOrganisation>()
                .Where(uo => uo.UserId == userId)
                .Where(uo => uo.OrganisationId == organisationId)
                .FirstOrDefault();

            if (userOrganisation == null)
            {
                throw new RegistrationNotFoundException();
            }

            return userOrganisation;
        }

        private void PopulateViewModelFromUserOrganisation(AdminPendingRegistrationViewModel viewModel, UserOrganisation userOrganisation)
        {
            viewModel.UserOrganisation = userOrganisation;

            if (userOrganisation.Organisation.Status == OrganisationStatuses.Pending)
            {
                string query = userOrganisation.Organisation.OrganisationName;
                AddOrganisationSeparateSearchResults searchResults = addOrganisationSearchService.SearchPrivateWithSeparateResults(query);

                viewModel.SimilarOrganisationsFromOurDatabase = searchResults.SearchResultsFromOurDatabase;
                viewModel.SimilarOrganisationsFromCompaniesHouse = searchResults.SearchResultsFromCompaniesHouse;
            }
        }

        private void ApproveRegistration(UserOrganisation userOrganisation)
        {
            // Confirm the registration
            userOrganisation.PINConfirmedDate = VirtualDateTime.Now;

            if (userOrganisation.Organisation.Status == OrganisationStatuses.Pending)
            {
                User adminUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

                userOrganisation.Organisation.SetStatus(
                    OrganisationStatuses.Active,
                    adminUser.UserId,
                    "Manually registered");
            }

            // Send an email to notify the user who has just been approved
            string returnUrl = Url.Action("ManageOrganisationsGet", "ManageOrganisations", null, "https");
            string registeringUserEmailAddress = userOrganisation.User.EmailAddress;
            emailSendingService.SendOrganisationRegistrationApprovedEmail(registeringUserEmailAddress, returnUrl);
            
            // Send an email to any other existing users registered to this organisation 
            EmailSendingServiceHelpers.SendUserAddedEmailToExistingUsers(userOrganisation.Organisation, userOrganisation.User, emailSendingService);
            
            // Log the approval
            auditLogger.AuditChangeToOrganisation(
                AuditedAction.RegistrationLog,
                userOrganisation.Organisation,
                new
                {
                    Status = "Manually registered",
                    Sector = userOrganisation.Organisation.SectorType,
                    Organisation = userOrganisation.Organisation.OrganisationName,
                    CompanyNo = userOrganisation.Organisation.CompanyNumber,
                    Address = userOrganisation.Organisation.GetLatestAddress()?.GetAddressString(),
                    SicCodes = userOrganisation.Organisation.GetSicCodeIdsString(),
                    UserFirstname = userOrganisation.User.Firstname,
                    UserLastname = userOrganisation.User.Lastname,
                    UserJobtitle = userOrganisation.User.JobTitle,
                    UserEmail = userOrganisation.User.EmailAddress,
                    userOrganisation.User.ContactPhoneNumber
                },
                User);
            
            dataRepository.SaveChanges();
        }

        private void RejectRegistration(UserOrganisation userOrganisation,
            AdminPendingRegistrationViewModel viewModel)
        {
            // Send an email to notify the applicant their request has been rejected
            string reason = !string.IsNullOrWhiteSpace(viewModel.RejectionReason)
                ? viewModel.RejectionReason
                : "We haven't been able to verify your employer's identity. So we have declined your application.";
            emailSendingService.SendOrganisationRegistrationDeclinedEmail(userOrganisation.User.EmailAddress, reason);

            // Log the rejection
            auditLogger.AuditChangeToOrganisation(
                AuditedAction.RegistrationLog,
                userOrganisation.Organisation,
                new
                {
                    Status = "Manually Rejected",
                    Reason = reason,
                    Sector = userOrganisation.Organisation.SectorType,
                    Organisation = userOrganisation.Organisation.OrganisationName,
                    CompanyNo = userOrganisation.Organisation.CompanyNumber,
                    Address = userOrganisation?.Organisation.GetLatestAddress()?.GetAddressString(),
                    SicCodes = userOrganisation.Organisation.GetSicCodeIdsString(),
                    UserFirstname = userOrganisation.User.Firstname,
                    UserLastname = userOrganisation.User.Lastname,
                    UserJobtitle = userOrganisation.User.JobTitle,
                    UserEmail = userOrganisation.User.EmailAddress,
                    userOrganisation.User.ContactPhoneNumber
                },
                User);
            
            // Delete the Organisation if it was manually created by this registration
            if (userOrganisation.Organisation.Status == OrganisationStatuses.Pending
                && userOrganisation.Organisation.UserOrganisations.Count == 1)
            {
                userOrganisation.Organisation.SetStatus(OrganisationStatuses.Deleted, details: "Manually Rejected");
            }
            
            // Delete this registration request
            dataRepository.Delete(userOrganisation);
            
            dataRepository.SaveChanges();
        }

    }
}
