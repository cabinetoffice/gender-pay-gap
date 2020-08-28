using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Services;

namespace GenderPayGap.WebUI.Repositories
{
    public class RegistrationRepository
    {

        private readonly IDataRepository dataRepository;
        private readonly AuditLogger auditLogger;

        public RegistrationRepository(
            IDataRepository dataRepository,
            AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }

        public async Task RemoveRetiredUserRegistrationsAsync(User userToRetire)
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
            await dataRepository.SaveChangesAsync();
        }

        public async Task RemoveRegistrationAsync(UserOrganisation userOrgToUnregister, User actionByUser)
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
            await dataRepository.SaveChangesAsync();

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.RegistrationLog,
                organisationToLog,
                new { Status = status },
                userOrgToUnregister.User);
        }


    }
}
