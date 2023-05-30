using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Repositories
{
    public class RegistrationRepository
    {

        private readonly IDataRepository dataRepository;
        private readonly AuditLogger auditLogger;
        private readonly PinInThePostService pinInThePostService;
        private readonly EmailSendingService emailSendingService;

        public RegistrationRepository(
            IDataRepository dataRepository,
            AuditLogger auditLogger,
            PinInThePostService pinInThePostService,
            EmailSendingService emailSendingService)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
            this.pinInThePostService = pinInThePostService;
            this.emailSendingService = emailSendingService;
        }

        public void RemoveRetiredUserRegistrations(User userToRetire)
        {
            // We extract this list of Organisations BEFORE deleting the UserOrganisations to prevent an Entity Framework error:
            // "Attempted to update or delete an entity that does not exist in the store."
            List<Organisation> organisationsToAuditLog = userToRetire.UserOrganisations
                .Select(userOrg => userOrg.Organisation)
                .ToList();

            foreach (UserOrganisation userOrgToUnregister in userToRetire.UserOrganisations)
            {
                // remove all the user registrations associated with the organisation
                userOrgToUnregister.Organisation.UserOrganisations.Remove(userOrgToUnregister);

                // Remove user organisation
                dataRepository.Delete(userOrgToUnregister);
            }

            foreach (Organisation organisation in organisationsToAuditLog)
            {
                // log unregistered via closed account
                auditLogger.AuditChangeToOrganisation(
                    AuditedAction.RegistrationLog,
                    organisation,
                    new { Status = "Unregistered closed account" },
                    userToRetire);
            }

            // save changes to database
            dataRepository.SaveChanges();
        }

        public void RemoveRegistration(UserOrganisation userOrgToUnregister, User actionByUser)
        {
            if (userOrgToUnregister is null)
            {
                throw new ArgumentNullException(nameof(userOrgToUnregister));
            }

            if (actionByUser is null)
            {
                throw new ArgumentNullException(nameof(actionByUser));
            }

            Organisation sourceOrg = userOrgToUnregister.Organisation;

            // Remove the user registration from the organisation
            sourceOrg.UserOrganisations.Remove(userOrgToUnregister);

            // We extract these two variables BEFORE deleting the UserOrganisations to prevent an Entity Framework error:
            // "Attempted to update or delete an entity that does not exist in the store."
            Organisation organisationToLog = userOrgToUnregister.Organisation;
            string status = (userOrgToUnregister.UserId == actionByUser.UserId) ? "Unregistered self" : "Unregistered";

            // Remove user organisation
            dataRepository.Delete(userOrgToUnregister);

            // Save changes to database
            dataRepository.SaveChanges();

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.RegistrationLog,
                organisationToLog,
                new { Status = status },
                userOrgToUnregister.User);
        }


        public UserOrganisation CreateRegistration(Organisation organisation, User user, IUrlHelper urlHelper)
        {
            var userOrganisation = new UserOrganisation
            {
                User = user,
                Organisation = organisation,
            };

            DecideRegistrationMethod(userOrganisation);

            if (userOrganisation.Method == RegistrationMethods.PinInPost)
            {
                bool pitpSuccess = pinInThePostService.GenerateAndSendPinInThePostAndUpdateUserOrganisationWithLetterId(userOrganisation, urlHelper);

                if (!pitpSuccess)
                {
                    // Sending a Pin In The Post failed
                    // Switch to Manual registration
                    userOrganisation.Method = RegistrationMethods.Manual;
                }
            }

            // Note: this is an IF, not an ELSE-IF, because we might change registration methods if PITP fails
            if (userOrganisation.Method == RegistrationMethods.Manual)
            {
                if (FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.SendRegistrationReviewEmails))
                {
                    SendReviewRegistrationEmailToGeo(userOrganisation, urlHelper);
                }
            }

            dataRepository.Insert(userOrganisation);
            dataRepository.SaveChanges();

            return userOrganisation;
        }

        private static void DecideRegistrationMethod(UserOrganisation userOrganisation)
        {
            if (userOrganisation.Organisation.Status == OrganisationStatuses.Pending)
            {
                // Organisations will have the "Pending" status if they have been added via manual data entry (and thus should be manually reviewed)
                // (Organisations will be "Active" if they already exist in our database, or if they are imported from CoHo)
                userOrganisation.Method = RegistrationMethods.Manual;
            }
            else if (userOrganisation.Organisation.SectorType == SectorTypes.Public)
            {
                userOrganisation.Method = RegistrationMethods.Manual;
            }
            else if (FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.PrivateManualRegistration))
            {
                userOrganisation.Method = RegistrationMethods.Manual;
            }
            else if (userOrganisation.Organisation.GetLatestAddress()?.IsUkAddress != true)
            {
                userOrganisation.Method = RegistrationMethods.Manual;
            }
            else
            {
                userOrganisation.Method = RegistrationMethods.PinInPost;
            }
        }

        private void SendReviewRegistrationEmailToGeo(UserOrganisation userOrganisation, IUrlHelper urlHelper)
        {
            User user = userOrganisation.User;
            Organisation organisation = userOrganisation.Organisation;

            string contactName = $"{user.Fullname} ({user.JobTitle})";
            string reportingAddress = organisation.GetLatestAddress()?.GetAddressString();

            string reviewUrl = urlHelper.Action("PendingRegistrationGet", "AdminPendingRegistrations", new { userId = userOrganisation.UserId, organisationId = userOrganisation.OrganisationId }, "https");

            emailSendingService.SendGeoOrganisationRegistrationRequestEmail(
                contactName,
                organisation.OrganisationName,
                reportingAddress,
                reviewUrl);
        }

    }
}
